using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string prefix = nameof(LoggingBehavior<TRequest, TResponse>);

        _logger.LogInformation(
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
            _logger.LogWarning(
                "[{PerfPossible}] The request '{RequestData}' took '{TimeTaken}' seconds",
                prefix,
                typeof(TRequest).Name,
                timeTaken.Seconds
            );
        }
        else
        {
            _logger.LogInformation(
                "[{PerfPossible}] The request '{RequestData}' took '{TimeTaken}' seconds",
                prefix,
                typeof(TRequest).Name,
                timeTaken.Seconds
            );
        }

        _logger.LogInformation("[{Prefix}] Handled '{RequestData}'", prefix, typeof(TRequest).Name);

        return response;
    }
}

public class StreamLoggingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<StreamLoggingBehavior<TRequest, TResponse>> _logger;

    public StreamLoggingBehavior(ILogger<StreamLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        const string prefix = nameof(StreamLoggingBehavior<TRequest, TResponse>);

        _logger.LogInformation(
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
                _logger.LogWarning(
                    "[{PerfPossible}] The request '{RequestData}' took '{TimeTaken}' seconds",
                    prefix,
                    typeof(TRequest).Name,
                    timeTaken.Seconds
                );
            }

            _logger.LogInformation("[{Prefix}] Handled '{RequestData}'", prefix, typeof(TRequest).Name);
            yield return response;
        }
    }
}
