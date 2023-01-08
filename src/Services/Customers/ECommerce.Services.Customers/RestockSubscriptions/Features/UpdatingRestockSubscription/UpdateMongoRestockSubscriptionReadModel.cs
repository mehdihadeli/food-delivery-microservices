using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Data;
using MongoDB.Driver;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.UpdatingRestockSubscription;

public record UpdateMongoRestockSubscriptionReadModel
    (RestockSubscription RestockSubscription, bool IsDeleted) : InternalCommand;

internal class UpdateMongoRestockSubscriptionReadModelHandler : ICommandHandler<UpdateMongoRestockSubscriptionReadModel>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public UpdateMongoRestockSubscriptionReadModelHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(
        UpdateMongoRestockSubscriptionReadModel command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var filterDefinition =
            Builders<RestockSubscriptionReadModel>.Filter
                .Eq(x => x.RestockSubscriptionId, command.RestockSubscription.Id.Value);

        var updateDefinition =
            Builders<RestockSubscriptionReadModel>.Update
                .Set(x => x.Email, command.RestockSubscription.Email.Value)
                .Set(x => x.ProductName, command.RestockSubscription.ProductInformation.Name)
                .Set(x => x.ProductId, command.RestockSubscription.ProductInformation.Id.Value)
                .Set(x => x.Processed, command.RestockSubscription.Processed)
                .Set(x => x.ProcessedTime, command.RestockSubscription.ProcessedTime)
                .Set(x => x.CustomerId, command.RestockSubscription.CustomerId.Value)
                .Set(x => x.IsDeleted, command.IsDeleted)
                .Set(x => x.RestockSubscriptionId, command.RestockSubscription.Id.Value);

        await _customersReadDbContext.RestockSubscriptions.UpdateOneAsync(
            filterDefinition,
            updateDefinition,
            new UpdateOptions(),
            cancellationToken);

        // await _customersReadDbContext.RestockSubscriptions.ReplaceOneAsync(
        //     x => x.RestockSubscriptionId == command.RestockSubscription.InternalCommandId.Value,
        //     updatedEntity,
        //     new ReplaceOptions(),
        //     cancellationToken);

        return Unit.Value;
    }
}
