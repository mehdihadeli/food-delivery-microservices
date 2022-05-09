using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.CQRS.Query;

namespace BuildingBlocks.Core.CQRS.Query;

public record PageRequest : IPageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public IList<string>? Includes { get; init; }
    public IList<FilterModel>? Filters { get; init; }
    public IList<string>? Sorts { get; init; }
}
