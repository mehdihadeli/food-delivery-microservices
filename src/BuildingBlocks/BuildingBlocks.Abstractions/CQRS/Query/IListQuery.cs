namespace BuildingBlocks.Abstractions.CQRS.Query;

public interface IListQuery<out TResponse> : IQuery<TResponse>
    where TResponse : notnull
{
    public IList<string>? Includes { get; init; }
    public IList<FilterModel>? Filters { get; init; }
    public IList<string>? Sorts { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
