using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCookbook.Application;
using MyCookbook.Application.Recipes;
using MyCookbook.Domain.Entities;
using MyCookbook.Infrastructure.Persistence;
using static MyCookbook.Application.Recipes.IExternalRecipeClient;

namespace MyCookbook.Infrastructure.ApplicationServices;

public sealed class RecipeService : IRecipeService
{
    private const string SourceMealDb = "MealDB";
    private readonly ApplicationDbContext _db;
    private readonly IExternalRecipeClient _external;
    private readonly AppOptions _app;
    private readonly IMemoryCache _cache;

    public RecipeService(ApplicationDbContext db, IExternalRecipeClient external)
    {
        _db = db;
        _external = external;
    }

    private static string BuildSearchKey(string query)
    {
        return $"mealdb:search:{query.Trim().ToLowerInvariant()}"; //needs invariant
    }

    public async Task<IReadOnlyList<ExternalSearchResultDto>> SearchExternalAsync(
        string query,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<ExternalSearchResultDto>();

        var key = BuildSearchKey(query);
        if (
            _cache.TryGetValue(key, out IReadOnlyList<ExternalSearchResultDto>? cached)
            && cached is not null
        )
            return cached;

        var hits = await _external.SearchAsync(query, ct);
        if (hits.Count == 0)
        {
            _cache.Set(
                key,
                Array.Empty<ExternalSearchResultDto>(),
                TimeSpan.FromSeconds(Math.Max(1, _app.Cache.ExternalSearchSeconds))
            );

            return Array.Empty<ExternalSearchResultDto>();
        }

        var ids = hits.Select(h => h.ExternalId).ToArray();
        var existingIds = await _db
            .Recipes.AsNoTracking()
            .Where(r =>
                r.ExternalSource == SourceMealDb
                && r.ExternalId != null
                && ids.Contains(r.ExternalId)
            )
            .Select(r => r.ExternalId!)
            .ToListAsync(ct);

        var existingSet =
            existingIds.Count > 0 ? new HashSet<string>(existingIds) : new HashSet<string>();

        var mapped = hits.Select(h => new ExternalSearchResultDto(
                ExternalId: h.ExternalId,
                Name: h.Name,
                ImageUrl: h.ImageUrl,
                IsSaved: existingSet.Contains(h.ExternalId)
            ))
            .ToList();

        //caching mapped results
        _cache.Set(
            key,
            mapped,
            TimeSpan.FromSeconds(Math.Max(1, _app.Cache.ExternalSearchSeconds))
        );

        return mapped;
    }

    public void InvalidateExternalSearchCache(string queryPrefix) //check later
    {
        var key = BuildSearchKey(queryPrefix);
        _cache.Remove(key);
    }

    public async Task<ImportResult> SaveFromExternalAsync(
        string externalId,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("externalId is required", nameof(externalId));

        var existing = await _db
            .Recipes.Where(r => r.ExternalSource == SourceMealDb && r.ExternalId == externalId)
            .Select(r => new { r.Id })
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
            return new ImportResult(existing.Id, Created: false);

        var dto = await _external.GetByIdAsync(externalId, ct);
        if (dto is null)
            throw new KeyNotFoundException($"External recipe '{externalId}' not found.");

        var ingredients = dto
            .Ingredients.Select(i => new RecipeIngredient(i.Name, i.Measure))
            .ToList();

        var entity = Recipe.FromExternal(
            externalId: dto.ExternalId,
            externalSource: SourceMealDb,
            name: dto.Name,
            instructions: dto.Instructions,
            imageUrl: dto.ImageUrl,
            ingredients: ingredients,
            prepMinutes: 0
        );

        _db.Recipes.Add(entity);
        await _db.SaveChangesAsync(ct);
        //TEST THIS
        InvalidateExternalSearchCacheForImport(dto);
        InvalidateExternalSearchCache(dto.Name);
        InvalidateExternalSearchCache(dto.ExternalId);
        return new ImportResult(entity.Id, Created: true);
    }

    public void InvalidateExternalSearchCacheForImport(ExternalRecipeDto dto) //invalidates common search keys
    {
        if (dto is null)
            return;
        InvalidateExternalSearchCacheForImport(dto.Name, dto.ExternalId);
    }

    private void InvalidateExternalSearchCacheForImport(string name, string? externalId)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var n = name.Trim();
            _cache.Remove(BuildSearchKey(n));

            var first = n.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first))
                _cache.Remove(BuildSearchKey(first));

            var compact = new string(n.Where(c => !char.IsWhiteSpace(c)).ToArray());
            if (compact.Length > 0)
                _cache.Remove(BuildSearchKey(compact));
        }
        if (!string.IsNullOrWhiteSpace(externalId))
        {
            _cache.Remove(BuildSearchKey(externalId.Trim()));
        }
    }

    // 3) Manual create
    public async Task<Guid> CreateManualAsync(RecipeCreateDto dto, CancellationToken ct = default)
    {
        var ingredients = dto
            .Ingredients.Select(i => new RecipeIngredient(i.Name, i.Measure))
            .ToList();

        var entity = Recipe.CreateManual(
            name: dto.Name,
            prepMinutes: dto.PrepMinutes,
            instructions: dto.Instructions,
            imageUrl: dto.ImageUrl,
            ingredients: ingredients
        );

        _db.Recipes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    // 4) Paged dashboard list with sorts
    public async Task<PagedResult<RecipeSummaryDto>> ListAsync(
        int page = 1,
        int pageSize = 20,
        RecipeSort sort = RecipeSort.CreatedAtDesc,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        var max = Math.Max(1, _app.Paging.MaxPageSize);
        pageSize = Math.Clamp(pageSize, 1, max);

        //var query = _db.Recipes.AsNoTracking();
        IQueryable<Recipe> query = _db.Recipes.AsNoTracking();

        query = sort switch
        {
            RecipeSort.CreatedAtAsc => query.OrderBy(r => r.CreatedAt),
            RecipeSort.PopularityDesc => query.OrderByDescending(r => r.Popularity),
            RecipeSort.PopularityAsc => query.OrderBy(r => r.Popularity),
            RecipeSort.NameAsc => query.OrderBy(r => r.Name),
            RecipeSort.NameDesc => query.OrderByDescending(r => r.Name),
            _ => query.OrderByDescending(r => r.CreatedAt),
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecipeSummaryDto(
                r.Id,
                r.Name,
                r.PrepMinutes,
                r.Popularity,
                r.ImageUrl,
                r.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<RecipeSummaryDto>(items, page, pageSize, total);
    }

    public async Task<RecipeDetailDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db
            .Recipes.AsNoTracking()
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (r is null)
            return null;

        var ingredients = r.Ingredients.Select(i => new IngredientDto(i.Name, i.Measure)).ToList();

        return new RecipeDetailDto(
            r.Id,
            r.Name,
            r.PrepMinutes,
            r.Popularity,
            r.ImageUrl,
            r.Instructions,
            ingredients,
            r.CreatedAt,
            r.UpdatedAt
        );
    }

    public RecipeService(
        ApplicationDbContext db,
        IExternalRecipeClient external,
        IMemoryCache cache,
        IOptions<AppOptions> app
    )
    {
        _db = db;
        _external = external;
        _cache = cache;
        _app = app.Value;
    }
}
