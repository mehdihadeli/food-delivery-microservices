using BuildingBlocks.Core.CQRS.Query;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomers;

// https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
// https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-6/
public record GetCustomersRequest : PageRequest
{
    // // For handling in minimal api
    // public static ValueTask<GetCustomersRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var page = httpContext.Request.Query.Get<int>("Page", 1);
    //     var pageSize = httpContext.Request.Query.Get<int>("PageSize", 20);
    //     var customerState = httpContext.Request.Query.Get<CustomerState>("CustomerState", CustomerState.None);
    //     var sorts = httpContext.Request.Query.GetCollection<List<string>>("Sorts");
    //     var filters = httpContext.Request.Query.GetCollection<List<FilterModel>>("Filters");
    //     var includes = httpContext.Request.Query.GetCollection<List<string>>("Includes");
    //
    //     var request = new GetCustomersRequest()
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         CustomerState = customerState,
    //         Sorts = sorts,
    //         Filters = filters,
    //         Includes = includes
    //     };
    //
    //     return ValueTask.FromResult<GetCustomersRequest?>(request);
    // }
}
