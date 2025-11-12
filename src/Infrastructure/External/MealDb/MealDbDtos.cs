// src/Infrastructure/External/MealDb/MealDbDtos.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyCookbook.Infrastructure.External.MealDb;

// { "meals": [ { ... } ] }
public sealed class MealDbSearchResponse
{
    [JsonPropertyName("meals")]
    public List<MealDbMeal>? Meals { get; set; }
}

public sealed class MealDbMeal
{
    public string idMeal { get; set; } = default!;
    public string strMeal { get; set; } = default!;
    public string? strInstructions { get; set; }
    public string? strMealThumb { get; set; }

    // Catch-all for all other properties from TheMealDB,(INGREDIENT-MEASURMENT pairs)
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}
