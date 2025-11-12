using CS101;

public interface IExternalRecipeClient
{
    // TODO-E7-1: add ct + paging + read-only list
    Task<IReadOnlyList<Recipe>> SearchAsync(
        string query,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );
}
