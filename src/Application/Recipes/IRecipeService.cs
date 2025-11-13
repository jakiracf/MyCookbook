using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace MyCookbook.Application.Recipes;

public interface IRecipeService
{
    Task<IReadOnlyList<ExternalSearchResultDto>> SearchExternalAsync(string query, CancellationToken ct=default);
    Task<ImportResult> SaveFromExternalAsync(string externalId, CancellationToken ct = default);
    Task<Guid> CreateManualAsync(RecipeCreateDto dto, CancellationToken ct = default);
    Task<PagedResult<RecipeSummaryDto>> ListAsync(
        int page = 1,
        int pageSize = 20,
        RecipeSort sort = RecipeSort.CreatedAtDesc,
        CancellationToken ct = default
    );
    Task<RecipeDetailDto?> GetAsync(Guid id, CancellationToken ct = default);
}

//temp dto used for debugging
/*public sealed record RecipeDto(
    [property: Required, MaxLength(200)] string Name,
    [property: Range(0, int.MaxValue)] int PrepMinutes,
    string? Instructions,
    string? ImageUrl,
    IReadOnlyList<IngredientDto> Ingredients
);*/
public sealed record ExternalSearchResultDto(
    string ExternalId,
    string Name,
    string? ImageUrl,
    bool IsSaved
);
public sealed record RecipeCreateDto(                    //img url validation ADDED
    [param: Required, MaxLength(200)] string Name,
    [param: Range(0, int.MaxValue)] int PrepMinutes,
    [param: Url, MaxLength(500)] string? ImageUrl,    //crashes with 500, fix later
    [param: MaxLength(4000)] string? Instructions,
    IReadOnlyList<IngredientDto> Ingredients
);

public sealed record IngredientDto(
    [param: Required, MaxLength(100)] string Name,
    [param: MaxLength(100)] string Measure
);

public sealed record RecipeSummaryDto(
    Guid Id,
    string Name,
    int PrepMinutes,
    int Popularity,
    string? ImageUrl,
    DateTime CreatedAt
);
public sealed record RecipeDetailDto(
    Guid Id,
    string Name,
    int PrepMinutes,
    int Popularity,
    string? ImageUrl,
    string? Instructions,
    IReadOnlyList<IngredientDto> Ingredients,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);

public sealed record ImportResult(
    Guid Id,
    bool Created
);

public enum RecipeSort
{
    CreatedAtDesc=0,
    CreatedAtAsc=1,
    PopularityDesc=2,
    PopularityAsc=3,
    NameDesc=4,
    NameAsc=5
}
