using BuildingBlocks.Core.CQRS.Query;

namespace Store.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

// https://blog.codingmilitia.com/2022/01/03/getting-complex-type-as-simple-type-query-string-aspnet-core-api-controller/
// https://benfoster.io/blog/minimal-apis-custom-model-binding-aspnet-6/
public record GetRestockSubscriptionsRequest : PageRequest
{
    public IList<string> Emails { get; init; } = new List<string>();
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    // public static ValueTask<GetRestockSubscriptionsRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var page = httpContext.Request.Query.Get<int>("Page", 1);
    //     var pageSize = httpContext.Request.Query.Get<int>("PageSize", 20);
    //     var sorts = httpContext.Request.Query.GetCollection<List<string>>("Sorts");
    //     var filters = httpContext.Request.Query.GetCollection<List<FilterModel>>("Filters");
    //     var includes = httpContext.Request.Query.GetCollection<List<string>>("Includes");
    //     var emails = httpContext.Request.Query.GetCollection<List<string>>("Emails");
    //     var from = httpContext.Request.Query.Get<DateTime?>("From", null);
    //     var to = httpContext.Request.Query.Get<DateTime?>("To", null);
    //
    //     var request = new GetRestockSubscriptionsRequest
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         Sorts = sorts,
    //         Filters = filters,
    //         Includes = includes,
    //         Emails = emails,
    //         From = from,
    //         To = to,
    //     };
    //
    //     return ValueTask.FromResult<GetRestockSubscriptionsRequest?>(request);
    // }
}
