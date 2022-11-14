using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionsByEmails;

public class GetRestockSubscriptionsByEmailsEndpoints : EndpointBaseSync
    .WithRequest<GetRestockSubscriptionsByEmailsRequest?>
    .WithActionResult<IAsyncEnumerable<RestockSubscriptionDto>>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetRestockSubscriptionsByEmailsEndpoints(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet(
        $"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/by-emails",
        Name = "GetRestockSubscriptionsByEmails")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiVersion(1.0)]
    [SwaggerOperation(
        Summary = "Get Restock Subscriptions by emails.",
        Description = "Get Restock Subscriptions by emails.",
        OperationId = "GetRestockSubscriptionsByEmails",
        Tags = new[] { RestockSubscriptionsConfigs.Tag })]
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
