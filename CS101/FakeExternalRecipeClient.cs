using CS101;

public class FakeExternalRecipeClient : IExternalRecipeClient
{
    public async Task<IReadOnlyList<Recipe>> SearchAsync(
        string query,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default
    )
    {
        // TODO-E7-2: validate args
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.", nameof(query));
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        await Task.Delay(300, ct);

        var q = query.Trim(); //null-save queries
        var filtered = RecipeSamples
            .Seed()
            .Where(r => (r.Name ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.PrepMinutes);

        var pageItems = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList().AsReadOnly();

        return pageItems;
    }
}
