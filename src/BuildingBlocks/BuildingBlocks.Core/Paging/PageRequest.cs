namespace BuildingBlocks.Core.Paging;

using BuildingBlocks.Abstractions.Core.Paging;

// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records#characteristics-of-records
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record
public record PageRequest : IPageRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Filters { get; init; }
    public string? SortOrder { get; init; }

    public void Deconstruct(out int pageNumber, out int pageSize, out string? filters, out string? sortOrder)
    {
        pageNumber = PageNumber;
        pageSize = PageSize;
        filters = Filters;
        sortOrder = SortOrder;
    }
}
