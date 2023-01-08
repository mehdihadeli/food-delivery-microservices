using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionsByEmails.v1;

public class GetRestockSubscriptionsByEmailsEndpoints : EndpointBaseSync
    .WithRequest<GetRestockSubscriptionsByEmailsRequest?>
    .WithActionResult<IAsyncEnumerable<RestockSubscriptionDto>>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetRestockSubscriptionsByEmailsEndpoints(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    // We could use `SwaggerResponse` form `Swashbuckle.AspNetCore` package instead of `ProducesResponseType` for supporting custom description for status codes
    [ProducesResponseType(typeof(RestockSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StatusCodeProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(StatusCodeProblemDetails), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Get Restock Subscriptions by emails.",
        Description = "Get Restock Subscriptions by emails.",
        OperationId = "GetRestockSubscriptionsByEmails",
        Tags = new[] {RestockSubscriptionsConfigs.Tag})]
    [ApiVersion(1.0)]
    [HttpGet(
        $"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/by-emails",
        Name = "GetRestockSubscriptionsByEmails")]
    [Authorize(Roles = CustomersConstants.Role.Admin)]
    public override ActionResult<IAsyncEnumerable<RestockSubscriptionDto>> Handle(
        [FromQuery] GetRestockSubscriptionsByEmailsRequest? request)
    {
        Guard.Against.Null(request, nameof(request));

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetRestockSubscriptionsByEmailsEndpoints)))
        using (Serilog.Context.LogContext.PushProperty("Emails", request.Emails))
        {
            var result = _queryProcessor.SendAsync(new GetRestockSubscriptionsByEmails(request.Emails));

            return Ok(result);
        }
    }
}
