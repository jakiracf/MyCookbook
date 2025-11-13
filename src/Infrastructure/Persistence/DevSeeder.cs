using MyCookbook.Domain.Entities;

namespace MyCookbook.Infrastructure.Persistence;

//ai-generated seed for testing
public static class DevSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (db.Recipes.Any())
            return;

        var r1 = Recipe.FromExternal(
            externalId: "52771",
            externalSource: "MealDB",
            name: "Spicy Arrabiata Penne",
            instructions: "Boil pasta. Make sauce. Combine. Serve.",
            imageUrl: null,
            ingredients: new[]
            {
                new RecipeIngredient("Penne Rigate", "300g"),
                new RecipeIngredient("Garlic", "3 cloves"),
                new RecipeIngredient("Chilli flakes", "1 tsp"),
                new RecipeIngredient("Tomatoes", "400g"),
            },
            prepMinutes: 25
        );

        var r2 = Recipe.FromExternal(
            externalId: "52772",
            externalSource: "MealDB",
            name: "Teriyaki Chicken Casserole",
            instructions: "Bake with sauce until tender.",
            imageUrl: null,
            ingredients: new[]
            {
                new RecipeIngredient("Chicken", "500g"),
                new RecipeIngredient("Teriyaki sauce", "1/2 cup"),
                new RecipeIngredient("Rice", "200g"),
            },
            prepMinutes: 40
        );

        db.Recipes.AddRange(r1, r2);
        await db.SaveChangesAsync();
    }
}
