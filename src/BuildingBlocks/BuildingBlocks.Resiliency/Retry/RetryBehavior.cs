using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace BuildingBlocks.Resiliency.Retry;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IRetryableRequest<TRequest, TResponse>> _retryHandlers;

    public RetryBehavior(
        IEnumerable<IRetryableRequest<TRequest, TResponse>> retryHandlers,
        ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _retryHandlers = retryHandlers;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var retryHandler = _retryHandlers.FirstOrDefault();

        // var retryAttr = typeof(TRequest).GetCustomAttribute<RetryPolicyAttribute>();
        if (retryHandler == null)

            // No retry handler found, continue through pipeline
            return await next();

        var circuitBreaker = Policy<TResponse>
            .Handle<System.Exception>()
            .CircuitBreakerAsync(retryHandler.ExceptionsAllowedBeforeCircuitTrip, TimeSpan.FromMilliseconds(5000),
                (exception, things) => _logger.LogDebug("Circuit Tripped!"),
                () =>
                {
                });

        var retryPolicy = Policy<TResponse>
            .Handle<System.Exception>()
            .WaitAndRetryAsync(retryHandler.RetryAttempts, retryAttempt =>
            {
                var retryDelay = retryHandler.RetryWithExponentialBackoff
                    ? TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * retryHandler.RetryDelay)
                    : TimeSpan.FromMilliseconds(retryHandler.RetryDelay);

                _logger.LogDebug("Retrying, waiting {RetryDelay}...", retryDelay);

                return retryDelay;
            });

        var response = await retryPolicy.ExecuteAsync(() => next());

        return response;
    }
}
