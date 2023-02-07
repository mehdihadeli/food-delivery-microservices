using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Domain;
using ECommerce.Services.Customers.RestockSubscriptions.Features.UpdatingRestockSubscription;
using ECommerce.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime.v1;

public record DeleteRestockSubscriptionsByTime(DateTime? From = null, DateTime? To = null) : ITxCommand;

internal class DeleteRestockSubscriptionsByTimeHandler : ICommandHandler<DeleteRestockSubscriptionsByTime>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly ICommandProcessor _commandProcessor;
    private readonly ILogger<DeleteRestockSubscriptionsByTimeHandler> _logger;

    public DeleteRestockSubscriptionsByTimeHandler(
        CustomersDbContext customersDbContext,
        IDomainEventPublisher domainEventPublisher,
        ICommandProcessor commandProcessor,
        ILogger<DeleteRestockSubscriptionsByTimeHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _domainEventPublisher = domainEventPublisher;
        _commandProcessor = commandProcessor;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteRestockSubscriptionsByTime command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var exists = await _customersDbContext.RestockSubscriptions
            .Where(x => (command.From == null && command.To == null) ||
                        (command.From == null && x.Created <= command.To) ||
                        (command.To == null && x.Created >= command.From) ||
                        (x.Created >= command.From && x.Created <= command.To))
            .ToListAsync(cancellationToken: cancellationToken);

        if (exists.Any() == false)
            throw new RestockSubscriptionDomainException("Not found any items to delete");

        // instead of directly use of `UpdateMongoRestockSubscriptionsReadModelByTime` we can use this code
        // foreach (var restockSubscription in exists)
        // {
        //     restockSubscription.Delete();
        // }

        foreach (var restockSubscription in exists)
        {
            _customersDbContext.Entry(restockSubscription).State = EntityState.Deleted;
            _customersDbContext.Entry(restockSubscription.ProductInformation).State = EntityState.Unchanged;
        }

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("'{Count}' RestockSubscriptions removed.'", exists.Count);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandProcessor.SendAsync(
            new UpdateMongoRestockSubscriptionsReadModelByTime(command.From, command.To, IsDeleted: true),
            cancellationToken);

        return Unit.Value;
    }
}
