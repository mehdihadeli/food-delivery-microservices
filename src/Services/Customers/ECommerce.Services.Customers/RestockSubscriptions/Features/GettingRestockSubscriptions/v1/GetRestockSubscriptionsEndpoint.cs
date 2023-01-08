using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetRestockSubscriptionsEndpoint : EndpointBaseAsync
    .WithRequest<GetRestockSubscriptionsRequest?>
    .WithActionResult<GetRestockSubscriptionsResponse>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetRestockSubscriptionsEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    // We could use `SwaggerResponse` form `Swashbuckle.AspNetCore` package instead of `ProducesResponseType` for supporting custom description for status codes
    [ProducesResponseType(typeof(GetRestockSubscriptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StatusCodeProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(StatusCodeProblemDetails), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Getting Restock Subscriptions.",
        Description = "Getting Restock Subscriptions.",
        OperationId = "GetRestockSubscriptions",
        Tags = new[] {RestockSubscriptionsConfigs.Tag})]
    [HttpGet(RestockSubscriptionsConfigs.RestockSubscriptionsUrl, Name = "GetRestockSubscriptions")]
    [ApiVersion(1.0)]
    [Authorize(Roles = CustomersConstants.Role.Admin)]
    public override async Task<ActionResult<GetRestockSubscriptionsResponse>> HandleAsync(
        [FromQuery] GetRestockSubscriptionsRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetRestockSubscriptionsEndpoint)))
        {
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
}
