namespace MyCookbook.Domain.Entities;

public class Recipe
{
    // === Columns / state ===
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!; // required (<= 200)
    public int PrepMinutes { get; private set; }
    public string? Instructions { get; private set; }
    public string? ImageUrl { get; private set; }

    public string? ExternalId { get; private set; } // "52772"
    public string? ExternalSource { get; private set; } //  "MealDB"
    public int Popularity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Ingredients modeled in the sdperate table
    private readonly List<RecipeIngredient> _ingredients = new();
    public IReadOnlyList<RecipeIngredient> Ingredients => _ingredients;

    public Recipe() { }

    //recipe factory
    public static Recipe CreateManual(
        string name,
        int prepMinutes,
        string? instructions,
        string? imageUrl,
        IEnumerable<RecipeIngredient> ingredients)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length == 0 || name.Length > 200)
            throw new ArgumentException(
                "name is required and must be between 1 and 200 chars",
                nameof(name)
            );
        if (prepMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(prepMinutes), "prepMinutes must be > 0");
        instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
        if (instructions is { Length: > 4000 })
            throw new ArgumentOutOfRangeException(nameof(instructions), "max 4000 chars");

        imageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        if (imageUrl is { Length: > 500 })
            throw new ArgumentOutOfRangeException(nameof(imageUrl), "max 500 chars");

        var r = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            PrepMinutes = Math.Max(0, prepMinutes),
            Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
            ExternalId = null, //manual
            ExternalSource = null, //manual
            Popularity = 0,
            CreatedAt = DateTime.UtcNow,
        };
        r._ingredients.AddRange(ingredients.Where(i => !string.IsNullOrWhiteSpace(i.Name)));
        return r;
    }

    //entity from an external recipe (TheMealDB)
    public static Recipe FromExternal(
        string externalId,
        string externalSource,
        string name,
        string? instructions,
        string? imageUrl,
        IEnumerable<RecipeIngredient> ingredients,
        int prepMinutes = 0
    )
    {
        // exception handling, not needed for demo
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("externalId is required", nameof(externalId));
        if (string.IsNullOrWhiteSpace(externalSource))
            throw new ArgumentException("externalSource is required", nameof(externalSource));

        name = (name ?? string.Empty).Trim();
        if (name.Length == 0 || name.Length > 200)
            throw new ArgumentException(
                "name is required and must be between 1 and 200 chars",
                nameof(name)
            );
        if (prepMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(prepMinutes), "prepMinutes must be > 0");
        instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
        if (instructions is { Length: > 4000 })
            throw new ArgumentOutOfRangeException(nameof(instructions), "max 4000 chars");

        imageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        if (imageUrl is { Length: > 500 })
            throw new ArgumentOutOfRangeException(nameof(imageUrl), "max 500 chars");

        var r = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            PrepMinutes = Math.Max(0, prepMinutes),
            Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
            ExternalId = externalId,
            ExternalSource = externalSource,
            Popularity = 0,
            CreatedAt = DateTime.UtcNow,
        };
        r._ingredients.AddRange(ingredients.Where(i => !string.IsNullOrWhiteSpace(i.Name)));
        return r;
    }

    public void SetName(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length == 0 || name.Length > 200)
            throw new ArgumentOutOfRangeException(nameof(name), "name must be 1..200 chars");
        Name = name;
        Touch();
    }

    public void SetPrepMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes), "minutes must be > 0");
        PrepMinutes = minutes;
        Touch();
    }

    public void SetInstructions(string? text)
    {
        Instructions = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        Touch();
    }

    public void SetImageUrl(string? url)
    {
        ImageUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        Touch();
    }

    public void ReplaceIngredients(IEnumerable<RecipeIngredient> items)
    {
        _ingredients.Clear();
        _ingredients.AddRange(items.Where(i => !string.IsNullOrWhiteSpace(i.Name)));
        Touch();
    }

    public void IncrementPopularity(int by = 1)
    {
        if (by < 0)
            return;
        Popularity += by;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}

public class RecipeIngredient
{
    public int Key { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Measure { get; private set; }

    private RecipeIngredient() { } // EF

    public RecipeIngredient(string name, string? measure)
    {
        Name = name.Trim();
        if (Name.Length == 0 || Name.Length > 200)
            throw new ArgumentOutOfRangeException(nameof(name), "name must be 1..200 chars");
        Measure = string.IsNullOrWhiteSpace(measure) ? null : measure.Trim();
    }

}
