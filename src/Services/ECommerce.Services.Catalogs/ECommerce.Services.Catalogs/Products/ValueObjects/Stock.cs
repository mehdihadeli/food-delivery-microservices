using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

public record Stock
{
    public Stock? Null => null;

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

    public static Stock Create(int available, int restockThreshold, int maxStockThreshold)
    {
        var stock = new Stock
        {
            Available = Guard.Against.Negative(
                available,
                new ProductDomainException("Available stock cannot be negative.")),
            RestockThreshold = Guard.Against.NegativeOrZero(
                restockThreshold,
                new ProductDomainException("Restock threshold cannot be negative or zero.")),
            MaxStockThreshold = Guard.Against.NegativeOrZero(
                maxStockThreshold,
                new ProductDomainException("Max stock threshold cannot be negative or zero.")),
        };

        if (stock.Available > stock.MaxStockThreshold)
            throw new MaxStockThresholdReachedException("Available stock cannot be greater than max stock threshold.");

        return stock;
    }
}
