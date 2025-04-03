using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using Mediator;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public record CreateMongoRestockSubscriptionReadModels(
    long RestockSubscriptionId,
    long CustomerId,
    string CustomerName,
    long ProductId,
    string ProductName,
    string Email,
    bool Processed,
    DateTime? ProcessedTime = null
) : InternalCommand
{
    public bool IsDeleted { get; init; }
}

internal class CreateRestockSubscriptionReadModelHandler(ICustomersReadUnitOfWork unitOfWork)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<CreateMongoRestockSubscriptionReadModels>
{
    public async ValueTask<Unit> Handle(
        CreateMongoRestockSubscriptionReadModels command,
        CancellationToken cancellationToken
    )
    {
        command.NotBeNull();

        var readModel = command.ToRestockSubscription();

        await unitOfWork.RestockSubscriptionsRepository.AddAsync(readModel, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
