namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Address
{
    // EF
    private Address()
    {
    }

    public string Country { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string Detail { get; private set; } = default!;
    public PostalCode PostalCode { get; private set; } = default!;

    public static Address Empty => new();

    public static Address Of(string country, string city, string detail, PostalCode postalCode)
    {
        var address = new Address {Country = country, City = city, Detail = detail, PostalCode = postalCode};

        return address;
    }
}

public record PostalCode
{
    // EF
    // because it is public we don't use value in the parameter, and ef sets value through `Value` property setter
    public PostalCode()
    {
    }

    public string Value { get; init; } = default!;

    // validations should be placed here instead of constructor
    public static PostalCode Of(string postalCode) => new() {Value = postalCode};
    public static implicit operator string(PostalCode postalCode) => postalCode.Value;
}
