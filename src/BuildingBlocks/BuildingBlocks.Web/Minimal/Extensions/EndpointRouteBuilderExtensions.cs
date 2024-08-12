using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Web.Problem.HttpResults;
using Humanizer;
using MediatR;
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
        Func<TRequest, TCommand>? mapRequestToCommand = null
    )
        where TRequest : class
        where TCommand : ICommand
    {
        return builder
            .MapPost(pattern, Handle)
            .WithName(typeof(TCommand).Name)
            .WithDisplayName(typeof(TCommand).Name.Humanize())
            .WithSummaryAndDescription(typeof(TCommand).Name.Humanize(), typeof(TCommand).Name.Humanize());

        // we can't generalize all possible type results for auto generating open-api metadata, because it might show unwanted response type as metadata
        async Task<NoContent> Handle([AsParameters] HttpCommand<TRequest> requestParameters)
        {
            var (request, context, commandProcessor, mapper, cancellationToken) = requestParameters;

            var command = mapRequestToCommand is not null
                ? mapRequestToCommand(request)
                : mapper.Map<TCommand>(request);
            await commandProcessor.SendAsync(command, cancellationToken);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.NoContent();
        }
    }

    public static RouteHandlerBuilder MapCommandEndpoint<TRequest, TResponse, TCommand, TCommandResult>(
        this IEndpointRouteBuilder builder,
        string pattern,
        int statusCode,
        Func<TRequest, TCommand>? mapRequestToCommand = null,
        Func<TCommandResult, TResponse>? mapCommandResultToResponse = null,
        Func<TResponse, long>? getId = null
    )
        where TRequest : class
        where TResponse : class
        where TCommandResult : class
        where TCommand : ICommand<TCommandResult>
    {
        return builder
            .MapPost(pattern, Handle)
            .WithName(typeof(TCommand).Name)
            .WithDisplayName(typeof(TCommand).Name.Humanize())
            .WithSummaryAndDescription(typeof(TCommand).Name.Humanize(), typeof(TCommand).Name.Humanize());

        // https://github.com/dotnet/aspnetcore/issues/47630
        // we can't generalize all possible type results for auto generating open-api metadata, because it might show unwanted response type as metadata
        async Task<IResult> Handle([AsParameters] HttpCommand<TRequest> requestParameters)
        {
            var (request, context, commandProcessor, mapper, cancellationToken) = requestParameters;
            var host = $"{context.Request.Scheme}://{context.Request.Host}";

            var command = mapRequestToCommand is not null
                ? mapRequestToCommand(request)
                : mapper.Map<TCommand>(request);
            var res = await commandProcessor.SendAsync(command, cancellationToken);

            var response = mapCommandResultToResponse is not null
                ? mapCommandResultToResponse(res)
                : mapper.Map<TResponse>(res);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return statusCode switch
            {
                StatusCodes.Status201Created
                    => getId is { }
                        ? TypedResults.Created($"{host}{pattern}/{getId(response)}", response)
                        : TypedResults.Ok(response),
                StatusCodes.Status401Unauthorized => TypedResultsExtensions.UnAuthorizedProblem(),
                StatusCodes.Status500InternalServerError => TypedResultsExtensions.InternalProblem(),
                StatusCodes.Status202Accepted => TypedResults.Accepted(new Uri($"{host}{pattern}"), response),
                _ => TypedResults.Ok(response)
            };
        }
    }

    public static RouteHandlerBuilder MapQueryEndpoint<TRequestParameters, TResponse, TQuery, TQueryResult>(
        this IEndpointRouteBuilder builder,
        string pattern,
        Func<TRequestParameters, TQuery>? mapRequestToQuery = null,
        Func<TQueryResult, TResponse>? mapQueryResultToResponse = null
    )
        where TRequestParameters : IHttpQuery
        where TResponse : class
        where TQueryResult : class
        where TQuery : IQuery<TQueryResult>
    {
        return builder
            .MapGet(pattern, Handle)
            .WithName(typeof(TQuery).Name)
            .WithDisplayName(typeof(TQuery).Name.Humanize())
            .WithSummaryAndDescription(typeof(TQuery).Name.Humanize(), typeof(TQuery).Name.Humanize());

        // we can't generalize all possible type results for auto generating open-api metadata, because it might show unwanted response type as metadata
        async Task<Ok<TResponse>> Handle([AsParameters] TRequestParameters requestParameters)
        {
            var queryProcessor = requestParameters.QueryBus;
            var mapper = requestParameters.Mapper;
            var cancellationToken = requestParameters.CancellationToken;

            var query = mapRequestToQuery is not null
                ? mapRequestToQuery(requestParameters)
                : mapper.Map<TQuery>(requestParameters);

            var res = await queryProcessor.SendAsync(query, cancellationToken);

            var response = mapQueryResultToResponse is not null
                ? mapQueryResultToResponse(res)
                : mapper.Map<TResponse>(res);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-7.0#multiple-response-types
            return TypedResults.Ok(response);
        }
    }
}
