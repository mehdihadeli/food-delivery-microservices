using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Logging;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string prefix = nameof(LoggingBehavior<TRequest, TResponse>);

        logger.LogInformation(
            "[{Prefix}] Handle request '{RequestData}' and response '{ResponseData}'",
            prefix,
            typeof(TRequest).Name,
            typeof(TResponse).Name
        );

        var timer = new Stopwatch();
        timer.Start();

        var response = await next();

        timer.Stop();
        var timeTaken = timer.Elapsed;
        if (timeTaken.Seconds > 3)
        {
            logger.LogWarning(
                "[{PerfPossible}] The request '{RequestData}' took '{TimeTaken}' seconds",
                prefix,
                typeof(TRequest).Name,
                timeTaken.Seconds
            );
        }
        else
        {
            logger.LogInformation(
                "[{PerfPossible}] The request '{RequestData}' took '{TimeTaken}' seconds",
                prefix,
                typeof(TRequest).Name,
                timeTaken.Seconds
            );
        }

        logger.LogInformation("[{Prefix}] Handled '{RequestData}'", prefix, typeof(TRequest).Name);

        return response;
    }
}

public class StreamLoggingBehavior<TRequest, TResponse>(ILogger<StreamLoggingBehavior<TRequest, TResponse>> logger)
    : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string prefix = nameof(StreamLoggingBehavior<TRequest, TResponse>);

        logger.LogInformation(
            "[{Prefix}] Handle request '{RequestData}' and response '{ResponseData}'",
            prefix,
            typeof(TRequest).Name,
            typeof(TResponse).Name
        );

        var timer = new Stopwatch();
        timer.Start();

        await foreach (var response in next().WithCancellation(cancellationToken))
        {
            timer.Stop();
            var timeTaken = timer.Elapsed;
            if (timeTaken.Seconds > 3)
            {
                logger.LogWarning(
                    "[{PerfPossible}] The request '{RequestData}' took '{TimeTaken}' seconds",
                    prefix,
                    typeof(TRequest).Name,
                    timeTaken.Seconds
                );
            }

            logger.LogInformation("[{Prefix}] Handled '{RequestData}'", prefix, typeof(TRequest).Name);
            yield return response;
        }
    }
}
