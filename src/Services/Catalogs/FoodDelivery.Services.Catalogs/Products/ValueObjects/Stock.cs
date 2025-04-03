using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;

namespace FoodDelivery.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net
// Records are immutable reference types and their support equality comparison out of the box. They compare based on their properties, not by reference. Records also have a ready "ToString" method out of the box, that outputs all the properties in a readable way.
public record Stock
{
    // EF
    private Stock() { }

    /// <summary>
    /// Gets quantity in stock.
    /// </summary>
    public int Available { get; private set; }

    /// <summary>
    /// Gets available stock at which we should reorder.
    /// </summary>
    public int RestockThreshold { get; private set; }

    /// <summary>
    /// Gets maximum number of units that can be in-stock at any time (due to physicial/logistical constraints in warehouses).
    /// </summary>
    public int MaxStockThreshold { get; private set; }

    public static Stock Of(int available, int restockThreshold, int maxStockThreshold)
    {
        // validations should be placed here instead of constructor
        available.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();
        maxStockThreshold.NotBeNegativeOrZero();

        var stock = new Stock
        {
            Available = available,
            RestockThreshold = restockThreshold,
            MaxStockThreshold = maxStockThreshold,
        };

        if (stock.Available > stock.MaxStockThreshold)
            throw new MaxStockThresholdReachedException("Available stock cannot be greater than max stock threshold.");

        return stock;
    }

    public void Deconstruct(out int available, out int restockThreshold, out int maxStockThreshold) =>
        (available, restockThreshold, maxStockThreshold) = (Available, RestockThreshold, MaxStockThreshold);
}
