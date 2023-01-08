using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record ProductId : AggregateId
{
    // EF
    private ProductId(long value) : base(value)
    {
    }

    // validations should be placed here instead of constructor
    public static ProductId Of(long id) => new(Guard.Against.NegativeOrZero(id));

    public static implicit operator long(ProductId id) => id.Value;
}
