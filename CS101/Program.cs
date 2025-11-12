using CS101;
using System.ComponentModel.Design;
using System.Linq;
Console.WriteLine("==Excercise 1==");

Console.WriteLine("---------------");

Recipe recipe = new Recipe("Spaghetti Bolognese", new List<string> { "spaghetti", "bolognese", "parmesan" }, 10);

Recipe recipe2 = new Recipe();
 recipe.PrintSummary();
Console.WriteLine("---------------");
var pancakes = new Recipe ("Pancakes", 20);
pancakes.Ingredients.AddRange(new List<string> { "eggs", "flour", "milk" });
pancakes.PrintSummary();
Console.WriteLine("---------------");
var seeded = RecipeSamples.Seed();
Console.WriteLine($"Seeded recipes: {seeded.Count}");

var all = RecipeSamples.Seed();
all.Add(recipe);
all.Add(recipe2);
all.Add(pancakes);
//will try sql queries later
var filtered = all.Where(r =>
    r.Name != null &&
    r.Name.Contains("spaghetti", StringComparison.OrdinalIgnoreCase)
);
Console.WriteLine("---------------");
Console.WriteLine($"Filtered recipes: {filtered.Count()}");
Console.WriteLine("---------------");
var top3ByPrepTime = all.OrderByDescending(r => r.PrepMinutes).Take(3);
Console.WriteLine($"Top 3 recipes by prep time: {string.Join(", ", top3ByPrepTime.Select(r => r.Name))}");
Console.WriteLine("---------------");

var top3_names = top3ByPrepTime.Select(r => r.Name);
foreach (var name in top3_names)
{
    Console.WriteLine(name);
}
Console.WriteLine("---------------");

var client = new FakeExternalRecipeClient();
var results = await client.SearchAsync("s");
foreach (var result in results)
{
    Console.WriteLine(result.Describe());
}
Console.WriteLine("---------------");