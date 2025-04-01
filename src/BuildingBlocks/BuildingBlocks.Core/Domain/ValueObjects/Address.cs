using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public record Address
{
    // EF
    private Address() { }

    public string Country { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string Detail { get; private set; } = default!;
    public PostalCode PostalCode { get; private set; } = default!;

    public static Address Empty => new();

    public static Address Of(string country, string city, string detail, PostalCode postalCode)
    {
        var address = new Address
        {
            Country = country.NotBeNullOrWhiteSpace(),
            City = city.NotBeNullOrWhiteSpace(),
            Detail = detail.NotBeNullOrWhiteSpace(),
            PostalCode = postalCode.NotBeNull(),
        };

        return address;
    }

    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
    // https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
    public void Deconstruct(out string country, out string city, out string detail, out PostalCode postalCode) =>
        (country, city, detail, postalCode) = (Country, City, Detail, PostalCode);
}

public record PostalCode
{
    // EF
    // because it is public we don't use value in the parameter, and ef sets value through `Value` property setter
    public PostalCode() { }

    public string Value { get; init; } = default!;

    // validations should be placed here instead of constructor
    public static PostalCode Of(string postalCode) => new() { Value = postalCode.NotBeNullOrWhiteSpace() };

    public static implicit operator string(PostalCode postalCode) => postalCode.Value;
}
