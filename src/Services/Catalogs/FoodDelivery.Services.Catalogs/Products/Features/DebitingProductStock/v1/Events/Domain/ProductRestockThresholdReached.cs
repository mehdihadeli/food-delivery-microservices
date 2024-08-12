using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Shared.Contracts;

namespace FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.V1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
internal record ProductRestockThresholdReached(
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

    public override bool Equals(object obj)
    {
        return Equals(obj as ProductRestockThresholdReached);
    }
}

internal class ProductRestockThresholdReachedHandler : IDomainEventHandler<ProductRestockThresholdReached>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ProductRestockThresholdReachedHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public Task Handle(ProductRestockThresholdReached notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        // For example send an email to get more products
        return Task.CompletedTask;
    }
}
