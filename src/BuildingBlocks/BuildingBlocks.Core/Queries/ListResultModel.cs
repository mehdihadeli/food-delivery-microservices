namespace BuildingBlocks.Core.Queries;

public record ListResultModel<T>(IList<T> Items, long TotalItems, int Page, int PageSize)
    where T : notnull
{
    public static ListResultModel<T> Empty => new(Enumerable.Empty<T>().ToList(), 0, 0, 0);

    public static ListResultModel<T> Create(IList<T> items, long totalItems = 0, int page = 1, int pageSize = 20)
    {
        return new ListResultModel<T>(items, totalItems, page, pageSize);
    }

    public ListResultModel<U> Map<U>(Func<T, U> map)
        where U : notnull
    {
        return ListResultModel<U>.Create(Items.Select(map).ToList(), TotalItems, Page, PageSize);
    }
}
