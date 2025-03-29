namespace BuildingBlocks.Abstractions.Core.Paging;

public interface IPageRequest
{
    int PageNumber { get; init; }
    int PageSize { get; init; }
    string? Filters { get; init; }
    string? SortOrder { get; init; }

    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
    // https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
    public void Deconstruct(out int pageNumber, out int pageSize, out string? filters, out string? sortOrder) =>
        (pageNumber, pageSize, filters, sortOrder) = (PageNumber, PageSize, Filters, SortOrder);
}
