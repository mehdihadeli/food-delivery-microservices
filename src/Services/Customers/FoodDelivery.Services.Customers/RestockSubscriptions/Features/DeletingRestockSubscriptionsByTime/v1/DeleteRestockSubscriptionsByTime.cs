using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Domain;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime.v1;

public record DeleteRestockSubscriptionsByTime(DateTime? From = null, DateTime? To = null) : ITxCommand;

internal class DeleteRestockSubscriptionsByTimeHandler(
    CustomersDbContext customersDbContext,
    IDomainEventPublisher domainEventPublisher,
    ICommandBus commandBus,
    ILogger<DeleteRestockSubscriptionsByTimeHandler> logger
) : ICommandHandler<DeleteRestockSubscriptionsByTime>
{
    public async Task Handle(DeleteRestockSubscriptionsByTime command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var exists = await customersDbContext
            .RestockSubscriptions.Where(x =>
                (command.From == null && command.To == null)
                || (command.From == null && x.Created <= command.To)
                || (command.To == null && x.Created >= command.From)
                || (x.Created >= command.From && x.Created <= command.To)
            )
            .ToListAsync(cancellationToken: cancellationToken);

        if (exists.Count != 0 == false)
            throw new RestockSubscriptionDomainException("Not found any items to delete");

        // instead of directly use of `UpdateMongoRestockSubscriptionsReadModelByTime` we can use this code
        // foreach (var restockSubscription in exists)
        // {
        //     restockSubscription.Delete();
        // }
        foreach (var restockSubscription in exists)
        {
            customersDbContext.Entry(restockSubscription).State = EntityState.Deleted;
            customersDbContext.Entry(restockSubscription.ProductInformation).State = EntityState.Unchanged;
        }

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("'{Count}' RestockSubscriptions removed.'", exists.Count);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await commandBus.SendAsync(
            new UpdateMongoRestockSubscriptionsReadModelByTime(command.From, command.To, IsDeleted: true),
            cancellationToken
        );
    }
}
