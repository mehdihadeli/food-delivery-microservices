using System.Text.Json;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<RequestValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RequestValidationBehavior(
        IServiceProvider serviceProvider,
        ILogger<RequestValidationBehavior<TRequest, TResponse>> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var validator = _serviceProvider.GetService<IValidator<TRequest>>()!;
        if (validator is null)
            return await next();

        _logger.LogInformation(
            "[{Prefix}] Handle request={RequestData} and response={ResponseData}",
            nameof(RequestValidationBehavior<TRequest, TResponse>),
            typeof(TRequest).Name,
            typeof(TResponse).Name
        );

        _logger.LogDebug(
            "Handling {FullName} with content {Request}",
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request)
        );

        await validator.HandleValidationAsync(request, cancellationToken);

        var response = await next();

        _logger.LogInformation("Handled {FullName}", typeof(TRequest).FullName);
        return response;
    }
}

public class StreamRequestValidationBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<StreamRequestValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IValidator<TRequest> _validator;

    public StreamRequestValidationBehavior(
        IServiceProvider serviceProvider,
        ILogger<StreamRequestValidationBehavior<TRequest, TResponse>> logger
    )
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _validator = _serviceProvider.GetService<IValidator<TRequest>>()!;
        if (_validator is null)
        {
            await foreach (var response in next().WithCancellation(cancellationToken))
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
            JsonSerializer.Serialize(request)
        );

        _validator.HandleValidation(request);

        await foreach (var response in next().WithCancellation(cancellationToken))
        {
            yield return response;
            _logger.LogInformation("Handled {FullName}", typeof(TRequest).FullName);
        }
    }
}
