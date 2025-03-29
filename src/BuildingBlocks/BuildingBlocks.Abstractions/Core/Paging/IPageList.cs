namespace BuildingBlocks.Abstractions.Core.Paging;

public interface IPageList<T>
    where T : class
{
    int CurrentPageSize { get; }
    int CurrentStartIndex { get; }
    int CurrentEndIndex { get; }
    int TotalPages { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
    IReadOnlyList<T> Items { get; init; }
    int TotalCount { get; init; }
    int PageNumber { get; init; }
    int PageSize { get; init; }

    IPageList<TR> MapTo<TR>(Func<T, TR> map)
        where TR : class;
}
