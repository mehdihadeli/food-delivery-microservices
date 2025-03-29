using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record ProductStockDebited(
    long ProductId,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    int DebitQuantity
) : DomainEvent
{
    public static ProductStockDebited Of(
        long productId,
        int availableStock,
        int restockThreshold,
        int maxStockThreshold,
        int debitQuantity
    )
    {
        productId.NotBeNegativeOrZero();
        availableStock.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();
        maxStockThreshold.NotBeNegativeOrZero();
        debitQuantity.NotBeNegativeOrZero();

        return new ProductStockDebited(productId, availableStock, restockThreshold, maxStockThreshold, debitQuantity);

        // // Also if validation rules are more complex we can use `fluentvalidation`
        // return new ProductStockDebitedValidator().HandleValidation(
        //     new ProductStockDebited(
        //         productId,
        //         availableStock,
        //         restockThreshold,
        //         maxStockThreshold,
        //         debitQuantity
        //     )
        // );
    }
}

internal class ProductStockDebitedHandler : IDomainEventHandler<ProductStockDebited>
{
    public ValueTask Handle(ProductStockDebited notification, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
