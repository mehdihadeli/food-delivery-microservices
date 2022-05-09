using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.CQRS.Query;

namespace BuildingBlocks.Core.CQRS.Query;

public record ListQuery<TResponse> : IListQuery<TResponse>
    where TResponse : notnull
{
    public IList<string>? Includes { get; init; }
    public IList<FilterModel>? Filters { get; init; }
    public IList<string>? Sorts { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
