using BuildingBlocks.Abstractions.Queries;

namespace BuildingBlocks.Core.Queries;

public record ListQuery<TResponse> : IListQuery<TResponse>
    where TResponse : notnull
{
    public IList<string>? Includes { get; init; }
    public IList<string>? Sorts { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Filters { get; init; }
    public string? SortOrder { get; init; }
}
