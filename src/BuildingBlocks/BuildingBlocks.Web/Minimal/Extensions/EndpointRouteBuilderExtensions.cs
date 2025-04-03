using BuildingBlocks.Abstractions.Web.MinimalApi;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Web.Minimal.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder MapCommandEndpoint<TRequest, TCommand>(
        this IEndpointRouteBuilder builder,
        string pattern,
        Func<TRequest, TCommand> mapRequestToCommand
    )
        where TRequest : class
        where TCommand : Abstractions.Commands.ICommand
    {
        return builder
            .MapPost(pattern, Handle)
            .WithName(typeof(TCommand).Name)
            .WithDisplayName(typeof(TCommand).Name.Humanize())
            .WithSummary(typeof(TCommand).Name.Humanize())
            .WithDescription(typeof(TCommand).Name.Humanize());

        // we can't generalize all possible type results for auto generating open-api metadata, because it might show unwanted response type as metadata
        async Task<NoContent> Handle([AsParameters] HttpCommand<TRequest> requestParameters)
        {
            var (request, context, mediator, cancellationToken) = requestParameters;

            var command = mapRequestToCommand(request);
            await mediator.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.NoContent();
        }
    }

    public static RouteHandlerBuilder MapCommandEndpoint<TRequest, TResponse, TCommand, TCommandResult>(
        this IEndpointRouteBuilder builder,
        string pattern,
        int statusCode,
        Func<TRequest, TCommand> mapRequestToCommand,
        Func<TCommandResult, TResponse> mapCommandResultToResponse,
        Func<TResponse, Guid>? getId = null
    )
        where TRequest : class
        where TResponse : notnull
        where TCommandResult : notnull
        where TCommand : Abstractions.Commands.ICommand<TCommandResult>
    {
        return builder
            .MapPost(pattern, Handle)
            .WithName(typeof(TCommand).Name)
            .WithDisplayName(typeof(TCommand).Name.Humanize())
            .WithSummary(typeof(TCommand).Name.Humanize())
            .WithDescription(typeof(TCommand).Name.Humanize());

        // https://github.com/dotnet/aspnetcore/issues/47630
        // we can't generalize all possible type results for auto generating open-api metadata, because it might show unwanted response type as metadata
        async Task<IResult> Handle([AsParameters] HttpCommand<TRequest> requestParameters)
        {
            var (request, context, commandBus, ct) = requestParameters;
            var host = $"{context.Request.Scheme}://{context.Request.Host}";

            var command = mapRequestToCommand(request);
            var res = await commandBus.SendAsync(command, ct);

            var response = mapCommandResultToResponse(res);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return statusCode switch
            {
                StatusCodes.Status201Created => getId is { }
                    ? TypedResults.Created($"{host}{pattern}/{getId(response)}", response)
                    : TypedResults.Ok(response),
                StatusCodes.Status401Unauthorized => TypedResultsExtensions.UnAuthorizedProblem(),
                StatusCodes.Status500InternalServerError => TypedResultsExtensions.InternalProblem(),
                StatusCodes.Status202Accepted => TypedResults.Accepted(new Uri($"{host}{pattern}"), response),
                _ => TypedResults.Ok(response),
            };
        }
    }

    public static RouteHandlerBuilder MapQueryEndpoint<TRequestParameters, TResponse, TQuery, TQueryResult>(
        this IEndpointRouteBuilder builder,
        string pattern,
        Func<TRequestParameters, TQuery> mapRequestToQuery,
        Func<TQueryResult, TResponse> mapQueryResultToResponse
    )
        where TRequestParameters : IHttpQuery
        where TResponse : class
        where TQueryResult : class
        where TQuery : class, Abstractions.Queries.IQuery<TQueryResult>
    {
        return builder
            .MapGet(pattern, Handle)
            .WithName(typeof(TQuery).Name)
            .WithDisplayName(typeof(TQuery).Name.Humanize())
            .WithSummary(typeof(TQuery).Name.Humanize())
            .WithDescription(typeof(TQuery).Name.Humanize());

        // we can't generalize all possible type results for auto generating open-api metadata, because it might show unwanted response type as metadata
        async Task<Ok<TResponse>> Handle([AsParameters] TRequestParameters requestParameters)
        {
            var queryBus = requestParameters.QueryBus;
            var cancellationToken = requestParameters.CancellationToken;

            var query = mapRequestToQuery.Invoke(requestParameters);

            var res = await queryBus.SendAsync(query, cancellationToken);

            var response = mapQueryResultToResponse(res);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(response);
        }
    }
}
