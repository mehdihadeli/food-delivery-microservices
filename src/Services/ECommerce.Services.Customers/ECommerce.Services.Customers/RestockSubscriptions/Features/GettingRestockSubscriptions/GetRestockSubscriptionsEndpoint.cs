using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Query;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetRestockSubscriptionsEndpoint : EndpointBaseAsync
    .WithRequest<GetRestockSubscriptionsRequest?>
    .WithActionResult<GetRestockSubscriptionsResult>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetRestockSubscriptionsEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet(RestockSubscriptionsConfigs.RestockSubscriptionsUrl, Name = "GetRestockSubscriptions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Get Restock Subscriptions.",
        Description = "Get Restock Subscriptions.",
        OperationId = "GetRestockSubscriptions",
        Tags = new[] { RestockSubscriptionsConfigs.Tag })]
    [Authorize(Roles = CustomersConstants.Role.Admin)]
    public override async Task<ActionResult<GetRestockSubscriptionsResult>> HandleAsync(
        [FromQuery] GetRestockSubscriptionsRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        var result = await _queryProcessor.SendAsync(
            new GetRestockSubscriptions
            {
                Page = request.Page,
                Sorts = request.Sorts,
                PageSize = request.PageSize,
                Filters = request.Filters,
                Includes = request.Includes,
                Emails = request.Emails,
                From = request.From,
                To = request.To
            },
            cancellationToken);

        return Ok(result);
    }
}
