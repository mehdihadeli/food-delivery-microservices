using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;
using Unit = Mediator.Unit;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;

public record UpdateMongoRestockSubscriptionReadModel(
    long RestockSubscriptionId,
    long CustomerId,
    string Email,
    long ProductId,
    string ProductName,
    bool Processed,
    DateTime? ProcessedTime,
    bool IsDeleted = false
) : InternalCommand;

public class UpdateMongoRestockSubscriptionReadModelHandler(CustomersReadUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMongoRestockSubscriptionReadModel>
{
    public async ValueTask<Unit> Handle(
        UpdateMongoRestockSubscriptionReadModel command,
        CancellationToken cancellationToken
    )
    {
        command.NotBeNull();

        var existingSubscription = await unitOfWork.RestockSubscriptionsRepository.FindOneAsync(
            x => x.RestockSubscriptionId == command.RestockSubscriptionId,
            cancellationToken
        );

        if (existingSubscription is null)
        {
            throw new RestockSubscriptionNotFoundException(command.RestockSubscriptionId);
        }

        existingSubscription = existingSubscription with
        {
            Processed = command.Processed,
            CustomerId = command.CustomerId,
            ProductName = command.ProductName,
            ProductId = command.ProductId,
            Email = command.Email,
            ProcessedTime = command.ProcessedTime,
            IsDeleted = command.IsDeleted,
        };

        await unitOfWork.RestockSubscriptionsRepository.UpdateAsync(existingSubscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
