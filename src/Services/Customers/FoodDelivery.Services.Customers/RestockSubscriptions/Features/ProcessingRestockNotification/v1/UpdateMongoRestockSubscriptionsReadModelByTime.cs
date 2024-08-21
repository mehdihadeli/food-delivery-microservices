using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;

public record UpdateMongoRestockSubscriptionsReadModelByTime(DateTime? From, DateTime? To, bool IsDeleted = false)
    : InternalCommand;

internal class UpdateMongoRestockSubscriptionsReadModelByTimeHandler(CustomersReadUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMongoRestockSubscriptionsReadModelByTime>
{
    public async Task Handle(
        UpdateMongoRestockSubscriptionsReadModelByTime command,
        CancellationToken cancellationToken
    )
    {
        command.NotBeNull();

        var itemsToUpdate = await unitOfWork.RestockSubscriptionsRepository.FindAsync(
            x =>
                (command.From == null && command.To == null)
                || (command.From == null && x.Created <= command.To)
                || (command.To == null && x.Created >= command.From)
                || (x.Created >= command.From && x.Created <= command.To),
            cancellationToken
        );

        if (itemsToUpdate.Any() == false)
            return;

        foreach (var restockSubscriptionReadModel in itemsToUpdate)
        {
            var updatedItem = restockSubscriptionReadModel with { IsDeleted = command.IsDeleted };
            await unitOfWork.RestockSubscriptionsRepository.UpdateAsync(updatedItem, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
