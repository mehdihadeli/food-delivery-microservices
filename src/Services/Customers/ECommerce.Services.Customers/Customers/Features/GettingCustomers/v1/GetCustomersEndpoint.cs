using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomers.v1;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetCustomersEndpoint : EndpointBaseAsync
    .WithRequest<GetCustomersRequest?>
    .WithActionResult<GetCustomersResponse>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetCustomersEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet(CustomersConfigs.CustomersPrefixUri, Name = "GetCustomers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiVersion(1.0)]
    [SwaggerOperation(
        Summary = "Getting All Customers",
        Description = "Getting All Customers",
        OperationId = "GetCustomers",
        Tags = new[] {CustomersConfigs.Tag})]
    public override async Task<ActionResult<GetCustomersResponse>> HandleAsync(
        [FromQuery] GetCustomersRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetCustomersEndpoint)))
        {
            var result = await _queryProcessor.SendAsync(
                new GetCustomers
                {
                    Filters = request.Filters,
                    Includes = request.Includes,
                    Page = request.Page,
                    Sorts = request.Sorts,
                    PageSize = request.PageSize
                },
                cancellationToken);

            return Ok(result);
        }
    }
}
