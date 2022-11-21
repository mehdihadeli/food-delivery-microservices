namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionsByEmails.v1;

public record GetRestockSubscriptionsByEmailsRequest(IList<string> Emails)
{
    // public static ValueTask<GetRestockSubscriptionsByEmailsRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    // {
    //     var emails = httpContext.Request.Query.GetCollection<List<string>>("Emails");
    //
    //     var request = new GetRestockSubscriptionsByEmailsRequest(emails);
    //
    //     return ValueTask.FromResult<GetRestockSubscriptionsByEmailsRequest?>(request);
    // }
}


