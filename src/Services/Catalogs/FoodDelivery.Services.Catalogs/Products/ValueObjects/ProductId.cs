using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net
// Records are immutable reference types and their support equality comparison out of the box. They compare based on their properties, not by reference. Records also have a ready "ToString" method out of the box, that outputs all the properties in a readable way.
public record ProductId : AggregateId
{
    // EF
    private ProductId(long value)
        : base(value) { }

    // validations should be placed here instead of constructor
    public static ProductId Of(long id) => new(id.NotBeNegativeOrZero());

    public static implicit operator long(ProductId id) => id.Value;
}
