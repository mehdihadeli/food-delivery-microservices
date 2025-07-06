using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net
// Records are immutable reference types and their support equality comparison out of the box. They compare based on their properties, not by reference. Records also have a ready "ToString" method out of the box, that outputs all the properties in a readable way.
public record Size
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private Size(string value)
    {
        Value = value;
    }

    // Note: in entities with none default constructor, for setting constructor parameter, we need a private set property
    // when we didn't define this property in fluent configuration mapping (if so we can remove private set) , because for getting mapping list of properties to set
    // in the constructor it should not be read only without set (for bypassing calculate fields)- https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
    public string Value { get; private set; }

    public static Size Of(string value)
    {
        // validations should be placed here instead of constructor
        value.NotBeNullOrWhiteSpace();
        return new Size(value);
    }

    public static implicit operator string(Size value) => value.Value;

    public void Deconstruct(out string value) => value = Value;
}
