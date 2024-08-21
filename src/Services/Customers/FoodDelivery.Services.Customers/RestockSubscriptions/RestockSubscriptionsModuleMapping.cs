using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using Riok.Mapperly.Abstractions;
using RestockSubscription = FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read.RestockSubscription;

namespace FoodDelivery.Services.Customers.RestockSubscriptions;

[Mapper]
internal static partial class RestockSubscriptionsModuleMapping
{
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.Id)}.{nameof(Models.Write.RestockSubscription.Id.Value)}",
        nameof(RestockSubscriptionDto.Id)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.Email)}.{nameof(Models.Write.RestockSubscription.Email.Value)}",
        nameof(RestockSubscriptionDto.Email)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.ProductInformation)}.{nameof(Models.Write.RestockSubscription.ProductInformation.Name)}",
        nameof(RestockSubscriptionDto.ProductName)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.ProductInformation)}.{nameof(Models.Write.RestockSubscription.ProductInformation.Id.Value)}",
        nameof(RestockSubscriptionDto.ProductId)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.CustomerId)}.{nameof(Models.Write.RestockSubscription.CustomerId.Value)}",
        nameof(RestockSubscriptionDto.CustomerId)
    )]
    internal static partial RestockSubscriptionDto ToRestockSubscriptionDto(
        this Models.Write.RestockSubscription restockSubscription
    );

    [MapperIgnoreTarget(nameof(RestockSubscription.Id))]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.Id)}.{nameof(Models.Write.RestockSubscription.Id.Value)}",
        nameof(RestockSubscription.RestockSubscriptionId)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.Email)}.{nameof(Models.Write.RestockSubscription.Email.Value)}",
        nameof(RestockSubscription.Email)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.ProductInformation)}.{nameof(Models.Write.RestockSubscription.ProductInformation.Name)}",
        nameof(RestockSubscription.ProductName)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.ProductInformation)}.{nameof(Models.Write.RestockSubscription.ProductInformation.Id.Value)}",
        nameof(RestockSubscription.ProductId)
    )]
    [MapProperty(
        $"{nameof(Models.Write.RestockSubscription.CustomerId)}.{nameof(Models.Write.RestockSubscription.CustomerId.Value)}",
        nameof(RestockSubscription.CustomerId)
    )]
    internal static partial RestockSubscription ToRestockSubscription(
        this Models.Write.RestockSubscription restockSubscription
    );

    [MapProperty(nameof(RestockSubscription.RestockSubscriptionId), nameof(RestockSubscriptionDto.Id))]
    internal static partial RestockSubscriptionDto ToRestockSubscriptionDto(
        this RestockSubscription restockSubscription
    );

    [MapperIgnoreTarget(nameof(RestockSubscription.Id))]
    [MapProperty(
        nameof(CreateMongoRestockSubscriptionReadModels.RestockSubscriptionId),
        nameof(RestockSubscription.RestockSubscriptionId)
    )]
    internal static partial RestockSubscription ToRestockSubscription(
        this CreateMongoRestockSubscriptionReadModels createMongoRestockSubscriptionReadModels
    );

    internal static partial IQueryable<RestockSubscriptionDto> ProjectToRestockSubscriptionDto(
        this IQueryable<RestockSubscription> queryable
    );
}
