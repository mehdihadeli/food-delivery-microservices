using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

internal record CreateMongoRestockSubscriptionReadModels(
    long RestockSubscriptionId,
    long CustomerId,
    string CustomerName,
    long ProductId,
    string ProductName,
    string Email,
    DateTime Created,
    bool Processed,
    DateTime? ProcessedTime = null
) : InternalCommand
{
    public bool IsDeleted { get; init; }
}

internal class CreateRestockSubscriptionReadModelHandler(CustomersReadUnitOfWork unitOfWork)
    : ICommandHandler<CreateMongoRestockSubscriptionReadModels>
{
    public async Task Handle(CreateMongoRestockSubscriptionReadModels command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var readModel = command.ToRestockSubscription();

        await unitOfWork.RestockSubscriptionsRepository.AddAsync(readModel, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
