namespace CS101;

public class Recipe : Entity, IPrintable
{
    public string Name { get; set; }
    public List<string> Ingredients { get; set; }
    public int PrepMinutes { get; set; }

    public Recipe(string name, List<string> ingredients, int prepMinutes)
    {
        Name = name;
        Ingredients = ingredients;
        PrepMinutes = prepMinutes;
    }

    public Recipe(string name, int prepMinutes)
    {
        Name = name;
        Ingredients = new List<string>();
        PrepMinutes = prepMinutes;
    }

    //only here if lazy-testing
    public Recipe()
    {
        Name = "defaultName";
        Ingredients = new List<string> { "defaultIngredients", "defCarrots", "defPotatoes" };
        PrepMinutes = 10;
    }

    public override string ToString()
    {
        return $"{Name}: {string.Join(", ", Ingredients)}";
    }

    public override string Describe()
    {
        return $"{base.Describe()} {Name}: {string.Join(", ", Ingredients)}";
    }

    public void PrintSummary()
    {
        Console.WriteLine(
            $"{Name}: Add {string.Join(", ", Ingredients)} and cook for {PrepMinutes} minutes"
        );
    }
}
