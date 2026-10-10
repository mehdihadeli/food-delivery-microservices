using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using Mediator;
using Squirrel.Core.Commands;
using Squirrel.Core.Extensions;

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

public class CreateRestockSubscriptionReadModelHandler(ICustomersReadUnitOfWork unitOfWork)
    : Squirrel.Abstractions.Commands.ICommandHandler<CreateMongoRestockSubscriptionReadModels>
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
