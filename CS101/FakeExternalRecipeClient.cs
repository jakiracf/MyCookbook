using CS101;

public class FakeExternalRecipeClient : IExternalRecipeClient
{
    public async Task<List<Recipe>> SearchAsync(string query)
    {
        await Task.Delay(300);
        var filtered = RecipeSamples.Seed().Where(r =>
        r.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        return filtered.ToList();
    }
}