using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Shared.Contracts;

namespace FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record ProductRestockThresholdReached(
    long ProductId,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    int Quantity
) : DomainEvent
{
    public static ProductRestockThresholdReached Of(
        long productId,
        int availableStock,
        int restockThreshold,
        int maxStockThreshold,
        int quantity
    )
    {
        productId.NotBeNegativeOrZero();
        availableStock.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();
        maxStockThreshold.NotBeNegativeOrZero();
        quantity.NotBeNegativeOrZero();

        return new ProductRestockThresholdReached(
            productId,
            availableStock,
            restockThreshold,
            maxStockThreshold,
            quantity
        );
    }
}

public class ProductRestockThresholdReachedHandler(ICatalogDbContext catalogDbContext)
    : IDomainEventHandler<ProductRestockThresholdReached>
{
    private readonly ICatalogDbContext _catalogDbContext = catalogDbContext;

    public ValueTask Handle(ProductRestockThresholdReached notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        return ValueTask.CompletedTask;
    }
}
