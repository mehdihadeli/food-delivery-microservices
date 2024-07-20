using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;

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

internal class UpdateMongoRestockSubscriptionReadModelHandler : ICommandHandler<UpdateMongoRestockSubscriptionReadModel>
{
    private readonly CustomersReadUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateMongoRestockSubscriptionReadModelHandler(CustomersReadUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateMongoRestockSubscriptionReadModel command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var existingSubscription = await _unitOfWork.RestockSubscriptionsRepository.FindOneAsync(
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
            IsDeleted = command.IsDeleted
        };

        await _unitOfWork.RestockSubscriptionsRepository.UpdateAsync(existingSubscription, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
