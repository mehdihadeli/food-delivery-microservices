using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Customers.RestockSubscriptions;

[Mapper]
internal static partial class RestockSubscriptionsMappings
{
    [MapProperty(nameof(RestockSubscriptionReadModel.RestockSubscriptionId), nameof(RestockSubscriptionDto.Id))]
    [MapperIgnoreSource(nameof(RestockSubscriptionReadModel.Id))]
    [MapperIgnoreSource(nameof(RestockSubscriptionReadModel.IsDeleted))]
    [MapperIgnoreSource(nameof(RestockSubscriptionReadModel.CreatedBy))]
    internal static partial RestockSubscriptionDto ToRestockSubscriptionDto(
        this RestockSubscriptionReadModel restockSubscriptionReadModel
    );

    [MapperIgnoreTarget(nameof(RestockSubscriptionReadModel.Id))]
    [MapperIgnoreTarget(nameof(RestockSubscriptionReadModel.CreatedBy))]
    [MapperIgnoreSource(nameof(CreateMongoRestockSubscriptionReadModels.InternalCommandId))]
    [MapperIgnoreSource(nameof(CreateMongoRestockSubscriptionReadModels.Type))]
    [MapProperty(
        nameof(CreateMongoRestockSubscriptionReadModels.OccurredOn),
        nameof(RestockSubscriptionReadModel.Created)
    )]
    internal static partial RestockSubscriptionReadModel ToRestockSubscription(
        this CreateMongoRestockSubscriptionReadModels createMongoRestockSubscriptionReadModels
    );

    internal static partial IQueryable<RestockSubscriptionDto> ToRestockSubscriptionsDto(
        this IQueryable<RestockSubscriptionReadModel> restockSubscriptionsReadModel
    );
}
