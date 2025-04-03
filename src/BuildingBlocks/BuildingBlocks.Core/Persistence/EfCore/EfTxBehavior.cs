using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BuildingBlocks.Core.Persistence.EfCore;

// Ref: https://github.com/thangchung/clean-architecture-dotnet/blob/main/src/N8T.Infrastructure.EfCore/TxBehavior.cs
public class EfTxBehavior<TRequest, TResponse>(
    IDbFacadeResolver dbFacadeResolver,
    ILogger<EfTxBehavior<TRequest, TResponse>> logger,
    IDomainEventPublisher domainEventPublisher,
    IDomainEventsAccessor domainEventsAccessor
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next
    )
    {
        // tx should not be called for queries
        if (message is not ITxRequest)
            return await next(message, cancellationToken);

        logger.LogInformation(
            "{Prefix} Handled command {MediatrRequest}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName
        );

        logger.LogDebug(
            "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
            nameof(EfTxBehavior<TRequest, TResponse>),
            typeof(TRequest).FullName,
            JsonSerializer.Serialize(message)
        );

        var strategy = dbFacadeResolver.Database.CreateExecutionStrategy();

        var result = await strategy.ExecuteAsync(async () =>
        {
            // https://www.thinktecture.com/en/entity-framework-core/use-transactionscope-with-caution-in-2-1/
            // https://github.com/dotnet/efcore/issues/6233#issuecomment-242693262
            var isInnerTransaction = dbFacadeResolver.Database.CurrentTransaction is not null;
            var transaction =
                dbFacadeResolver.Database.CurrentTransaction
                ?? await dbFacadeResolver.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next(message, cancellationToken);

                logger.LogInformation(
                    "{Prefix} Open the transaction for {MediatrRequest}",
                    nameof(EfTxBehavior<TRequest, TResponse>),
                    typeof(TRequest).FullName
                );

                var domainEvents = domainEventsAccessor.DequeueUncommittedDomainEvents();
                await domainEventPublisher.PublishAsync(domainEvents.ToArray(), cancellationToken);

                if (isInnerTransaction == false)
                {
                    await transaction.CommitAsync(cancellationToken);
                    logger.LogInformation(
                        "{Prefix} Transaction for {MediatrRequest} completed.",
                        nameof(EfTxBehavior<TRequest, TResponse>),
                        typeof(TRequest).FullName
                    );
                }

                return response;
            }
            catch
            {
                if (isInnerTransaction == false)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogInformation(
                        "{Prefix} Transaction for {MediatrRequest} rolled back.",
                        nameof(EfTxBehavior<TRequest, TResponse>),
                        typeof(TRequest).FullName
                    );
                }

                throw;
            }
        });

        return result;
    }
}
