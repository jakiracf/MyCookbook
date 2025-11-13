using Microsoft.EntityFrameworkCore;
using MyCookbook.Application.Recipes;
using MyCookbook.Domain.Entities;
using MyCookbook.Infrastructure.Persistence;

namespace MyCookbook.Infrastructure.ApplicationServices;

public sealed class RecipeService : IRecipeService
{
    private const string SourceMealDb = "MealDB";

    private readonly ApplicationDbContext _db;
    private readonly IExternalRecipeClient _external;

    public RecipeService(ApplicationDbContext db, IExternalRecipeClient external)
    {
        _db = db;
        _external = external;
    }
    public async Task<IReadOnlyList<ExternalSearchResultDto>> SearchExternalAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<ExternalSearchResultDto>();

        var hits = await _external.SearchAsync(query, ct);
        if (hits.Count == 0)
            return Array.Empty<ExternalSearchResultDto>();

        var ids = hits.Select(h => h.ExternalId).ToArray();

        var existingIds = await _db.Recipes
            .AsNoTracking()
            .Where(r => r.ExternalSource == SourceMealDb && r.ExternalId != null && ids.Contains(r.ExternalId))
            .Select(r => r.ExternalId!)
            .ToListAsync(ct);

        var set = existingIds.Count > 0
            ? new HashSet<string>(existingIds)
            : new HashSet<string>();

        return hits.Select(h => new ExternalSearchResultDto(
            ExternalId: h.ExternalId,
            Name: h.Name,
            ImageUrl: h.ImageUrl,
            IsSaved: set.Contains(h.ExternalId)
        )).ToList();
    }
    public async Task<ImportResult> SaveFromExternalAsync(string externalId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("externalId is required", nameof(externalId));

        var existing = await _db.Recipes
            .Where(r => r.ExternalSource == SourceMealDb && r.ExternalId == externalId)
            .Select(r => new { r.Id })
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
            return  new ImportResult(existing.Id, Created: false);

        var dto = await _external.GetByIdAsync(externalId, ct);
        if (dto is null)
            throw new KeyNotFoundException($"External recipe '{externalId}' not found.");

        var ingredients = dto.Ingredients.Select(i => new RecipeIngredient(i.Name, i.Measure)).ToList();

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
        return new ImportResult(entity.Id, Created: true);
    }

    // 3) Manual create
    public async Task<Guid> CreateManualAsync(RecipeCreateDto dto, CancellationToken ct = default)
    {
        var ingredients = dto.Ingredients.Select(i => new RecipeIngredient(i.Name, i.Measure)).ToList();

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
        int page = 1, int pageSize = 20, RecipeSort sort = RecipeSort.CreatedAtDesc, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Recipe> query = _db.Recipes.AsNoTracking();

        query = sort switch
        {
            RecipeSort.CreatedAtAsc   => query.OrderBy(r => r.CreatedAt),
            RecipeSort.PopularityDesc => query.OrderByDescending(r => r.Popularity),
            RecipeSort.PopularityAsc  => query.OrderBy(r => r.Popularity),
            RecipeSort.NameAsc        => query.OrderBy(r => r.Name),
            RecipeSort.NameDesc       => query.OrderByDescending(r => r.Name),
            _                         => query.OrderByDescending(r => r.CreatedAt),
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecipeSummaryDto(
                r.Id, r.Name, r.PrepMinutes, r.Popularity, r.ImageUrl, r.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<RecipeSummaryDto>(items, page, pageSize, total);
    }
    public async Task<RecipeDetailDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.Recipes
            .AsNoTracking()
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (r is null) return null;

        var ingredients = r.Ingredients
            .Select(i => new IngredientDto(i.Name, i.Measure))
            .ToList();

        return new RecipeDetailDto(
            r.Id, r.Name, r.PrepMinutes, r.Popularity, r.ImageUrl, r.Instructions,
            ingredients, r.CreatedAt, r.UpdatedAt);
    }
}
