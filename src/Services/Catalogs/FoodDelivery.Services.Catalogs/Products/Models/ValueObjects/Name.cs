using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://enterprisecraftsmanship.com/posts/functional-c-primitive-obsession/
// https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net
// Records are immutable reference types and their support equality comparison out of the box. They compare based on their properties, not by reference. Records also have a ready "ToString" method out of the box, that outputs all the properties in a readable way.
public record Name
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private Name() { }

    public string Value { get; private set; } = default!;

    public static Name Of(string value)
    {
        // validations should be placed here instead of constructor
        value.NotBeNullOrWhiteSpace();

        return new Name { Value = value };
    }

    public static implicit operator string(Name value) => value.Value;

    public void Deconstruct(out string value) => value = Value;
}
