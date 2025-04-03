using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ReplenishingProductStock.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record ProductStockReplenished(
    long ProductId,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    int ReplenishedQuantity
) : DomainEvent
{
    public static ProductStockReplenished Of(
        long productId,
        int availableStock,
        int restockThreshold,
        int maxStockThreshold,
        int replenishedQuantity
    )
    {
        productId.NotBeNegativeOrZero();
        availableStock.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();
        maxStockThreshold.NotBeNegativeOrZero();
        replenishedQuantity.NotBeNegativeOrZero();

        return new ProductStockReplenished(
            productId,
            availableStock,
            restockThreshold,
            maxStockThreshold,
            replenishedQuantity
        );

        // // Also if validation rules are more complex we can use `fluentvalidation`
        // return new ProductStockReplenishedValidator().HandleValidation(
        //     new ProductStockReplenished(
        //         productId,
        //         availableStock,
        //         restockThreshold,
        //         maxStockThreshold,
        //         replenishedQuantity
        //     )
        // );
    }
}

public class ProductStockReplenishedHandler : IDomainEventHandler<ProductStockReplenished>
{
    public ValueTask Handle(ProductStockReplenished notification, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
