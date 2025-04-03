using System.Text.Json;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Persistence.Mongo;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Persistence.Mongo;

/// <summary>
/// Hint: transaction not work on mongo standalone.
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

    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next
    )
    {
        if (message is not ITxRequest)
        {
            return await next(message, cancellationToken);
        }

        _logger.LogInformation(
            "{Prefix} Handled command {TRequest}",
            nameof(MongoTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName
        );
        _logger.LogDebug(
            "{Prefix} Handled command {TRequest} with content {RequestContent}",
            nameof(MongoTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(message)
        );
        _logger.LogInformation(
            "{Prefix} Open the transaction for {TRequest}",
            nameof(MongoTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName
        );

        try
        {
            // Achieving atomicity
            await _dbContext.BeginTransactionAsync(cancellationToken);

            var response = await next(message, cancellationToken);
            _logger.LogInformation(
                "{Prefix} Executed the {TRequest} request",
                nameof(MongoTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName
            );

            await _dbContext.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch (Exception e)
        {
            await _dbContext.RollbackTransaction(cancellationToken);
            throw;
        }
    }
}
