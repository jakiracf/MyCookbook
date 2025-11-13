namespace MyCookbook.Application.Recipes;

public interface IExternalRecipeClient
{
    Task<IReadOnlyList<ExternalRecipeDto>> SearchAsync(
        string query,
        CancellationToken ct = default
    );
    Task<ExternalRecipeDto?> GetByIdAsync(string externalId, CancellationToken ct = default); //get single

    public sealed record ExternalRecipeDto(
        string ExternalId,
        string Name,
        string? Instructions,
        string? ImageUrl,
        IReadOnlyList<ExternalIngredient> Ingredients
    );

    public sealed record ExternalIngredient(string Name, string? Measure);
}
