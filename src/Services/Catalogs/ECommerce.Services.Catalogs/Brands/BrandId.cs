using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Catalogs.Brands;

public record BrandId : AggregateId
{
    // EF
    private BrandId(long value) : base(value)
    {
    }

    public static implicit operator long(BrandId id) => id.Value;

    // validations should be placed here instead of constructor
    public static BrandId Of(long id) => new(Guard.Against.NegativeOrZero(id));
}
