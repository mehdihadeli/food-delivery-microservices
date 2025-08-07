using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Commands.Diagnostics;
using BuildingBlocks.Core.Queries.Diagnostics;
using Mediator;

namespace BuildingBlocks.Core.Pipelines;

public class DiagnosticsPipelineBehavior<TRequest, TResponse>(
    CommandHandlerActivity commandActivity,
    CommandHandlerMetrics commandMetrics,
    QueryHandlerActivity queryActivity,
    QueryHandlerMetrics queryMetrics
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next
    )
    {
        var isCommand = message is ICommandBase;
        var isQuery = message is IQueryBase;

        if (isCommand)
        {
            commandMetrics.StartExecuting<TRequest>();
        }

        if (isQuery)
        {
            queryMetrics.StartExecuting<TRequest>();
        }

        try
        {
            if (isCommand)
            {
                var commandResult = await commandActivity.Execute<TRequest, TResponse>(
                    async (activity, ct) =>
                    {
                        var response = await next(message, ct);

                        return response;
                    },
                    cancellationToken
                );

                commandMetrics.FinishExecuting<TRequest>();

                return commandResult;
            }

            if (isQuery)
            {
                var queryResult = await queryActivity.Execute<TRequest, TResponse>(
                    async (activity, ct) =>
                    {
                        var response = await next(message, ct);

                        return response;
                    },
                    cancellationToken
                );

                queryMetrics.FinishExecuting<TRequest>();

                return queryResult;
            }
        }
        catch (System.Exception)
        {
            if (isQuery)
            {
                queryMetrics.FailedQuery<TRequest>();
            }

            if (isCommand)
            {
                commandMetrics.FailedCommand<TRequest>();
            }

            throw;
        }

        return await next(message, cancellationToken);
    }
}
