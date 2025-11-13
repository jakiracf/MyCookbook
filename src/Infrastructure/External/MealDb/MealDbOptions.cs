namespace MyCookbook.Infrastructure.External.MealDb; //lowercase ahh </3

public sealed class MealDbOptions
{
    public string BaseUrl { get; set; } = "https://www.themealdb.com/api/json/v1";
    public string ApiKey { get; set; } = "1";
}
