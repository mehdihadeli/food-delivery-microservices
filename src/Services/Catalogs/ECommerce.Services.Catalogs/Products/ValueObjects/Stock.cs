using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Stock
{
    // EF
    private Stock()
    {
    }

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
        var stock = new Stock
        {
            Available = Guard.Against.Negative(available),
            RestockThreshold = Guard.Against.NegativeOrZero(restockThreshold),
            MaxStockThreshold = Guard.Against.NegativeOrZero(maxStockThreshold),
        };

        if (stock.Available > stock.MaxStockThreshold)
            throw new MaxStockThresholdReachedException("Available stock cannot be greater than max stock threshold.");

        return stock;
    }
}
