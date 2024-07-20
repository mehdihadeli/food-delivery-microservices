using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Customers.Products;

public record ProductId : AggregateId<long>
{
    // EF
    protected ProductId(long value)
        : base(value) { }

    public static implicit operator long(ProductId id) => id.Value;

    // validations should be placed here instead of constructor
    public static ProductId Of(long id) => new(id.NotBeNegativeOrZero());
}
