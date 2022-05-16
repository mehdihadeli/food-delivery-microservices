using BuildingBlocks.Core.CQRS.Query;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProducts;

// https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
// https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-6/
public record GetProductsRequest : PageRequest
{
    // public static ValueTask<GetProductsRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var page = httpContext.Request.Query.Get<int>("Page", 1);
    //     var pageSize = httpContext.Request.Query.Get<int>("PageSize", 20);
    //     var sorts = httpContext.Request.Query.GetCollection<List<string>>("Sorts");
    //     var filters = httpContext.Request.Query.GetCollection<List<FilterModel>>("Filters");
    //     var includes = httpContext.Request.Query.GetCollection<List<string>>("Includes");
    //
    //     var request = new GetProductsRequest
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         Sorts = sorts,
    //         Filters = filters,
    //         Includes = includes
    //     };
    //
    //     return ValueTask.FromResult<GetProductsRequest?>(request);
    // }
}
