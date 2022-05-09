using System.Text.Json;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.Mongo;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Persistence.Mongo;

/// <summary>
/// Hint: transaction not work on mongo standalone
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class MongoTxBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly IMongoDbContext _dbContext;
    private readonly ILogger<MongoTxBehavior<TRequest, TResponse>> _logger;

    public MongoTxBehavior(IMongoDbContext dbContext, ILogger<MongoTxBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbContext = dbContext;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (request is not ITxRequest)
        {
            return await next();
        }

        _logger.LogInformation("{Prefix} Handled command {MediatRRequest}",
            nameof(MongoTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName);
        _logger.LogDebug("{Prefix} Handled command {MediatRRequest} with content {RequestContent}",
            nameof(MongoTxBehavior<TRequest, TResponse>), typeof(TRequest).FullName, JsonSerializer.Serialize(request));
        _logger.LogInformation("{Prefix} Open the transaction for {MediatRRequest}",
            nameof(MongoTxBehavior<TRequest, TResponse>), typeof(TRequest).FullName);

        try
        {
            // Achieving atomicity
            await _dbContext.BeginTransactionAsync(cancellationToken);

            var response = await next();
            _logger.LogInformation("{Prefix} Executed the {MediatRRequest} request",
                nameof(MongoTxBehavior<TRequest, TResponse>), typeof(TRequest).FullName);

            await _dbContext.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch (System.Exception e)
        {
            await _dbContext.RollbackTransaction(cancellationToken);
            throw;
        }
    }
}
