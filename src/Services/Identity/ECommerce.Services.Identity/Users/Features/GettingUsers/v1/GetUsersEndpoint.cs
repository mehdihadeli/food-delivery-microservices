using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Users.Features.GettingUsers.v1;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetUsersEndpoint : EndpointBaseAsync
    .WithRequest<GetUsersRequest?>
    .WithActionResult<GetUsersResponse>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetUsersEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet(UsersConfigs.UsersPrefixUri, Name = "GetUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiVersion(1.0)]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Get all users",
        OperationId = "GetUsers",
        Tags = new[] { UsersConfigs.Tag })]
    public override async Task<ActionResult<GetUsersResponse>> HandleAsync(
        [FromQuery] GetUsersRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        var result = await _queryProcessor.SendAsync(
            new GetUsers
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
