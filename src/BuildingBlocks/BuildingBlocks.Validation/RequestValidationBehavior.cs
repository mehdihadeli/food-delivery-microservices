using System.Text.Json;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Validation;

public class RequestValidationBehavior<TRequest, TResponse>(
    IServiceProvider serviceProvider,
    ILogger<RequestValidationBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next
    )
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>()!;
        if (validator is null)
            return await next(message, cancellationToken);

        logger.LogInformation(
            "[{Prefix}] Handle request={RequestData} and response={ResponseData}",
            nameof(RequestValidationBehavior<TRequest, TResponse>),
            typeof(TRequest).Name,
            typeof(TResponse).Name
        );

        logger.LogDebug(
            "Handling {FullName} with content {Request}",
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(message)
        );

        await validator.HandleValidationAsync(message, cancellationToken);

        var response = await next(message, cancellationToken);

        logger.LogInformation("Handled {FullName}", typeof(TRequest).FullName);
        return response;
    }
}

public class StreamRequestValidationBehavior<TRequest, TResponse>(
    IServiceProvider serviceProvider,
    ILogger<StreamRequestValidationBehavior<TRequest, TResponse>> logger
) : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<StreamRequestValidationBehavior<TRequest, TResponse>> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider _serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        StreamHandlerDelegate<TRequest, TResponse> next
    )
    {
        var validator = _serviceProvider.GetService<IValidator<TRequest>>()!;

        if (validator is null)
        {
            await foreach (var response in next(message, cancellationToken))
            {
                yield return response;
            }

            yield break;
        }

        _logger.LogInformation(
            "[{Prefix}] Handle request={RequestData} and response={ResponseData}",
            nameof(StreamRequestValidationBehavior<TRequest, TResponse>),
            typeof(TRequest).Name,
            typeof(TResponse).Name
        );

        _logger.LogDebug(
            "Handling {FullName} with content {Request}",
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(message)
        );

        await validator.HandleValidationAsync(message, cancellationToken: cancellationToken);

        await foreach (var response in next(message, cancellationToken))
        {
            yield return response;
            _logger.LogInformation("Handled {FullName}", typeof(TRequest).FullName);
        }
    }
}
