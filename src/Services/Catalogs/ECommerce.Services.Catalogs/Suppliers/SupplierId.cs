using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Catalogs.Suppliers;

public record SupplierId : AggregateId
{
    // EF
    private SupplierId(long value) : base(value)
    {
    }

    // validations should be placed here instead of constructor
    public static SupplierId Of(long id) => new(Guard.Against.NegativeOrZero(id));

    public static implicit operator long(SupplierId id) => id.Value;
}
