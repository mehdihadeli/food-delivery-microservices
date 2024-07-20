using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;

public record UpdateMongoRestockSubscriptionsReadModelByTime(DateTime? From, DateTime? To, bool IsDeleted = false)
    : InternalCommand;

internal class UpdateMongoRestockSubscriptionsReadModelByTimeHandler
    : ICommandHandler<UpdateMongoRestockSubscriptionsReadModelByTime>
{
    private readonly IMapper _mapper;
    private readonly CustomersReadUnitOfWork _unitOfWork;

    public UpdateMongoRestockSubscriptionsReadModelByTimeHandler(IMapper mapper, CustomersReadUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        UpdateMongoRestockSubscriptionsReadModelByTime command,
        CancellationToken cancellationToken
    )
    {
        command.NotBeNull();

        var itemsToUpdate = await _unitOfWork.RestockSubscriptionsRepository.FindAsync(
            x =>
                (command.From == null && command.To == null)
                || (command.From == null && x.Created <= command.To)
                || (command.To == null && x.Created >= command.From)
                || (x.Created >= command.From && x.Created <= command.To),
            cancellationToken
        );

        if (itemsToUpdate.Any() == false)
            return Unit.Value;

        foreach (var restockSubscriptionReadModel in itemsToUpdate)
        {
            var updatedItem = restockSubscriptionReadModel with { IsDeleted = command.IsDeleted };
            await _unitOfWork.RestockSubscriptionsRepository.UpdateAsync(updatedItem, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
