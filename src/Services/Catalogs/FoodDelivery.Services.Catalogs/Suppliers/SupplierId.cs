using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Suppliers;

public record SupplierId : AggregateId
{
    // EF
    private SupplierId(long value)
        : base(value) { }

    // validations should be placed here instead of constructor
    public static SupplierId Of(long id) => new(id.NotBeNegativeOrZero());

    public static implicit operator long(SupplierId id) => id.Value;
}
