namespace MyCookbook.Application;

public sealed class AppOptions
{
    public PagingOptions Paging { get; init; } = new();
    public CachingOtions Cache { get; init; } = new();
    public PopularityOptions Popularity { get; init; } = new();
}

public sealed class PagingOptions
{
    public int MaxPageSize { get; init; } = 50;
}

public sealed class CachingOtions
{
    public int ExternalSearchSeconds { get; init; } = 20;
}

public sealed class PopularityOptions
{
    public int OnImport { get; init; } = 5;
    public int OnView { get; init; } = 1;
}
