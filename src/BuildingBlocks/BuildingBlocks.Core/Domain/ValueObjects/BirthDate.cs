using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public record BirthDate
{
    // EF Core
    private BirthDate() { }

    public DateTime Value { get; private set; }

    public static BirthDate Of([NotNull] DateTime? value)
    {
        value.NotBeNull();

        return Of(value.Value);
    }

    public static BirthDate Of(DateTime value)
    {
        // validations should be placed here instead of constructor
        if (value == default)
        {
            throw new DomainException($"BirthDate {value} cannot be null");
        }

        DateTime minDateOfBirth = DateTime.Now.AddYears(-115);
        DateTime maxDateOfBirth = DateTime.Now.AddYears(-15);

        // Validate the minimum age.
        if (value < minDateOfBirth || value > maxDateOfBirth)
        {
            throw new DomainException("The minimum age has to be 15 years.");
        }

        return new BirthDate { Value = value };
    }

    public static implicit operator DateTime(BirthDate value) => value.Value;

    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
    // https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
    public void Deconstruct(out DateTime value) => value = Value;
}
