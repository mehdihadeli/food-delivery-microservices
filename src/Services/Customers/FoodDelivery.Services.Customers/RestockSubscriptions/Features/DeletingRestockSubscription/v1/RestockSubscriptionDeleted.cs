using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

public record RestockSubscriptionDeleted(long RestockSubscriptionId) : DomainEvent;

internal class RestockSubscriptionDeletedHandler : IDomainEventHandler<RestockSubscriptionDeleted>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IMapper _mapper;
    private readonly CustomersDbContext _customersDbContext;

    public RestockSubscriptionDeletedHandler(
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CustomersDbContext customersDbContext
    )
    {
        _commandProcessor = commandProcessor;
        _mapper = mapper;
        _customersDbContext = customersDbContext;
    }

    public async Task Handle(RestockSubscriptionDeleted notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();
        // var isDeleted = (bool)_customersDbContext.Entry(notification.RestockSubscription)
        //     .Property("IsDeleted")
        //     .CurrentValue!;

        var restockSubscription = await _customersDbContext.RestockSubscriptions.FirstOrDefaultAsync(
            x => x.Id == notification.RestockSubscriptionId,
            cancellationToken
        );

        if (restockSubscription is null)
            return;

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandProcessor.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(
                restockSubscription.Id,
                restockSubscription.CustomerId,
                restockSubscription.Email,
                restockSubscription.ProductInformation.Id,
                restockSubscription.ProductInformation.Name,
                restockSubscription.Processed,
                restockSubscription.ProcessedTime,
                true
            ),
            cancellationToken
        );
    }
}
