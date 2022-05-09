using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace BuildingBlocks.Resiliency.Fallback;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/

/// <summary>
/// MediatR Fallback Pipeline Behavior
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class FallbackBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IFallbackHandler<TRequest, TResponse>> _fallbackHandlers;
    private readonly ILogger<FallbackBehavior<TRequest, TResponse>> _logger;

    public FallbackBehavior(
        IEnumerable<IFallbackHandler<TRequest, TResponse>> fallbackHandlers,
        ILogger<FallbackBehavior<TRequest, TResponse>> logger)
    {
        _fallbackHandlers = fallbackHandlers;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var fallbackHandler = _fallbackHandlers.FirstOrDefault();
        if (fallbackHandler == null)

            // No fallback handler found, continue through pipeline
            return await next();

        var fallbackPolicy = Policy<TResponse>
            .Handle<Exception>()
            .FallbackAsync(ct =>
            {
                _logger.LogDebug(
                    $"Initial handler failed. Falling back to `{fallbackHandler.GetType().FullName}@HandleFallback`");
                return fallbackHandler.HandleFallbackAsync(request, cancellationToken);
            });

        var response = await fallbackPolicy.ExecuteAsync(() => next());

        return response;
    }
}