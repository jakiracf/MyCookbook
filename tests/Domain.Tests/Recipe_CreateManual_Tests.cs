using MyCookbook.Domain.Entities;
using Xunit;

namespace Domain.Tests;

public sealed class Recipe_CreateManual_Tests //minimal test so CI OKs
{
    [Fact]
    public void Create_entity_with_trimmed_name_and_ingredients()
    {
        var r = Recipe.CreateManual(
            name: "  Pancakes  ",
            prepMinutes: 10,
            instructions: "mix & fry",
            imageUrl: "https://example.com/p.jpg",
            ingredients: new()
            {
                new RecipeIngredient("Flour", "200 g"),
                new RecipeIngredient("Milk", "250 ml"),
            }
        );

        Assert.Equal("Pancakes", r.Name);
        Assert.Equal(10, r.PrepMinutes);
        Assert.Equal(2, r.Ingredients.Count);
    }
}
