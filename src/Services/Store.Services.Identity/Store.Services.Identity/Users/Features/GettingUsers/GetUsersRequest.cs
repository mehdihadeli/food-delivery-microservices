using BuildingBlocks.Core.CQRS.Query;

namespace Store.Services.Identity.Users.Features.GettingUsers;

public record GetUsersRequest : PageRequest
{
    // public static ValueTask<GetUsersRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var page = httpContext.Request.Query.Get<int>("Page", 1);
    //     var pageSize = httpContext.Request.Query.Get<int>("PageSize", 20);
    //     var sorts = httpContext.Request.Query.GetCollection<List<string>>("Sorts");
    //     var filters = httpContext.Request.Query.GetCollection<List<FilterModel>>("Filters");
    //     var includes = httpContext.Request.Query.GetCollection<List<string>>("Includes");
    //
    //     var request = new GetUsersRequest
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         Sorts = sorts,
    //         Filters = filters,
    //         Includes = includes
    //     };
    //
    //     return ValueTask.FromResult<GetUsersRequest?>(request);
    // }
}
