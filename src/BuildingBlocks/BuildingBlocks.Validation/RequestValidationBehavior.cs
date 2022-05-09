using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<RequestValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IValidator<TRequest> _validator;

    public RequestValidationBehavior(
        IServiceProvider serviceProvider,
        ILogger<RequestValidationBehavior<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _validator = _serviceProvider.GetService<IValidator<TRequest>>()!;
        if (_validator is null)
            return await next();

        _logger.LogInformation(
            "[{Prefix}] Handle request={X-RequestData} and response={X-ResponseData}",
            nameof(RequestValidationBehavior<TRequest, TResponse>),
            typeof(TRequest).Name,
            typeof(TResponse).Name);

        _logger.LogDebug(
            "Handling {FullName} with content {Request}",
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        await _validator.HandleValidationAsync(request, cancellationToken);

        var response = await next();

        _logger.LogInformation("Handled {FullName}", typeof(TRequest).FullName);
        return response;
    }
}

public class StreamRequestValidationBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IStreamRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<StreamRequestValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IValidator<TRequest> _validator;

    public StreamRequestValidationBehavior(
        IServiceProvider serviceProvider,
        ILogger<StreamRequestValidationBehavior<TRequest, TResponse>> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        StreamHandlerDelegate<TResponse> next)
    {
        _validator = _serviceProvider.GetService<IValidator<TRequest>>()!;
        if (_validator is null)
            return next();

        _logger.LogInformation(
            "[{Prefix}] Handle request={X-RequestData} and response={X-ResponseData}",
            nameof(StreamRequestValidationBehavior<TRequest, TResponse>),
            typeof(TRequest).Name,
            typeof(TResponse).Name);

        _logger.LogDebug(
            "Handling {FullName} with content {Request}",
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(request));

        _validator.HandleValidation(request);

        var response = next();

        _logger.LogInformation("Handled {FullName}", typeof(TRequest).FullName);
        return response;
    }
}
