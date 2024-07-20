using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;

namespace BuildingBlocks.Core.Paging;

public record PageList<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount) : IPageList<T>
    where T : class
{
    public int CurrentPageSize => Items.Count;
    public int CurrentStartIndex => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
    public int CurrentEndIndex => TotalCount == 0 ? 0 : CurrentStartIndex + CurrentPageSize - 1;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public static PageList<T> Empty => new(Enumerable.Empty<T>().ToList(), 0, 0, 0);

    public static PageList<T> Create(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalItems)
    {
        return new PageList<T>(items, pageNumber, pageSize, totalItems);
    }

    public IPageList<TR> MapTo<TR>(Func<T, TR> map)
        where TR : class
    {
        return PageList<TR>.Create(Items.Select(map).ToList(), PageNumber, PageSize, TotalCount);
    }

    public IPageList<TR> MapTo<TR>(IMapper mapper)
        where TR : class
    {
        return PageList<TR>.Create(mapper.Map<IReadOnlyList<TR>>(Items), PageNumber, PageSize, TotalCount);
    }
}
