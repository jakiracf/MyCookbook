namespace MyCookbook.Application.Recipes;

public interface IExternalRecipeClient
{
    Task<IReadOnlyList<ExternalRecipeDto>> SearchAsync(
        string query,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );

    public sealed record ExternalRecipeDto(
        string ExternalId,
        string Name,
        string? Instructions,
        string? ImageUrl,
        IReadOnlyList<ExternalIngredient> Ingredients
    );
    public sealed record ExternalIngredient(string Name, string? Measure);
}
