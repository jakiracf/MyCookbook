namespace CS101;

public class RecipeSamples
{
    public static List<Recipe> Seed() =>
        new() //remember ho +w to use lambdas!
        {
            new Recipe(
                "Spaghetti Bolognese",
                new List<string> { "spaghetti", "bolognese", "parmesan", "minced meat" },
                10
            ),
            new Recipe("Carbonara", new List<string> { "pasta", "bacon", "eggs" }, 10),
            new Recipe(
                "Tuna salad",
                new List<string> { "tuna", "lettuce", "tomatoes", "cabbage", "onions" },
                15
            ),
            new Recipe(
                "Chicken sandwich",
                new List<string> { "chicken", "bread", "mayo", "letuce" },
                5
            ),
            new Recipe(
                "Pizza",
                new List<string> { "tomatoes", "cheese", "pepperoni", "mushrooms" },
                30
            ),
            new Recipe("Hamburger", new List<string> { "beef", "cheese", "onions", "lettuce" }, 10),
            new Recipe(
                "Chicken soup",
                new List<string> { "chicken", "onions", "carrots", "celery" },
                20
            ),
            new Recipe(
                "Egg salad",
                new List<string> { "eggs", "lettuce", "tomatoes", "onions" },
                10
            ),
        };
}
