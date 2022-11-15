using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProducts;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
// https://im5tu.io/article/2022/09/asp.net-core-versioning-mvc-apis/
public class GetProductsEndpoint : EndpointBaseAsync
    .WithRequest<GetProductsRequest?>
    .WithActionResult<GetProductsResponse>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetProductsEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet(ProductsConfigs.ProductsPrefixUri, Name = "GetProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiVersion(1.0)]
    [SwaggerOperation(
        Summary = "Getting All Products",
        Description = "Getting All Products",
        OperationId = "GetProducts",
        Tags = new[] {ProductsConfigs.Tag})]
    public override async Task<ActionResult<GetProductsResponse>> HandleAsync(
        [FromQuery] GetProductsRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(GetProductsEndpoint)))
        {
            var result = await _queryProcessor.SendAsync(
                new GetProducts
                {
                    Page = request.Page,
                    Sorts = request.Sorts,
                    PageSize = request.PageSize,
                    Filters = request.Filters,
                    Includes = request.Includes,
                },
                cancellationToken);

            return Ok(result);
        }
    }
}
