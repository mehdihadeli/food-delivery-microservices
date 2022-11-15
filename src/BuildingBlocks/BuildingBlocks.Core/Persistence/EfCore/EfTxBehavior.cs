using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BuildingBlocks.Core.Persistence.EfCore;

// Ref: https://github.com/thangchung/clean-architecture-dotnet/blob/main/src/N8T.Infrastructure.EfCore/TxBehavior.cs
public class EfTxBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly IDbFacadeResolver _dbFacadeResolver;
    private readonly ILogger<EfTxBehavior<TRequest, TResponse>> _logger;
    private readonly ISerializer _serializer;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IDomainEventContext _domainEventContext;

    public EfTxBehavior(
        IDbFacadeResolver dbFacadeResolver,
        ILogger<EfTxBehavior<TRequest, TResponse>> logger,
        IDomainEventPublisher domainEventPublisher,
        ISerializer serializer,
        IDomainEventContext domainEventContext)
    {
        _dbFacadeResolver = dbFacadeResolver;
        _logger = logger;
        _serializer = serializer;
        _domainEventPublisher = domainEventPublisher;
        _domainEventContext = domainEventContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using (Serilog.Context.LogContext.PushProperty("RequestObject", _serializer.Serialize(request)))
        {
            if (request is not ITxRequest) return await next();

            _logger.LogInformation(
                "{Prefix} Handled command {MediatrRequest}",
                nameof(EfTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            _logger.LogDebug(
                "{Prefix} Handled command {MediatrRequest} with content {RequestContent}",
                nameof(EfTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName,
                JsonSerializer.Serialize(request));

            _logger.LogInformation(
                "{Prefix} Open the transaction for {MediatrRequest}",
                nameof(EfTxBehavior<TRequest, TResponse>),
                typeof(TRequest).FullName);

            var strategy = _dbFacadeResolver.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // https://www.thinktecture.com/en/entity-framework-core/use-transactionscope-with-caution-in-2-1/
                // https://github.com/dotnet/efcore/issues/6233#issuecomment-242693262
                var isInnerTransaction = _dbFacadeResolver.Database.CurrentTransaction is not null;
                var transaction = _dbFacadeResolver.Database.CurrentTransaction ??
                                  await _dbFacadeResolver.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var response = await next();

                    _logger.LogInformation(
                        "{Prefix} Executed the {MediatrRequest} request",
                        nameof(EfTxBehavior<TRequest, TResponse>),
                        typeof(TRequest).FullName);

                    var domainEvents = _domainEventContext.GetAllUncommittedEvents();
                    await _domainEventPublisher.PublishAsync(domainEvents.ToArray(), cancellationToken);

                    if (isInnerTransaction == false)
                        await transaction.CommitAsync(cancellationToken);

                    _domainEventContext.MarkUncommittedDomainEventAsCommitted();

                    return response;
                }
                catch
                {
                    if (isInnerTransaction == false)
                        await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
    }
}
