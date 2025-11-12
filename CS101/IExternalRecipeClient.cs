using CS101;

public interface IExternalRecipeClient
{
    Task<List<Recipe>> SearchAsync(string query);
}