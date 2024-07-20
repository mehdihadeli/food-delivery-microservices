using BuildingBlocks.Abstractions.Core.Paging;

namespace BuildingBlocks.Core.Paging;

// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records#characteristics-of-records
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record
public record PageRequest : IPageRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Filters { get; init; }
    public string? SortOrder { get; init; }

    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
    // https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
    public void Deconstruct(out int pageNumber, out int pageSize, out string? filters, out string? sortOrder) =>
        (pageNumber, pageSize, filters, sortOrder) = (PageNumber, PageSize, Filters, SortOrder);

    //// This handle with [AsParameters] .net 7
    // https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
    // https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-
    // public static ValueTask<PageRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var page = httpContext.Request.Query.Get<int>("PageNumber", 1);
    //     var pageSize = httpContext.Request.Query.Get<int>("PageSize", 20);
    //     var sorts = httpContext.Request.Query.Get<string>("SortOrder");
    //     var filters = httpContext.Request.Query.Get<string>("Filters");
    //
    //     var request = new PageRequest
    //     {
    //         PageNumber = page,
    //         PageSize = pageSize,
    //         SortOrder = sorts,
    //         Filters = filters,
    //     };
    //
    //     return ValueTask.FromResult<PageRequest?>(request);
    // }
}
