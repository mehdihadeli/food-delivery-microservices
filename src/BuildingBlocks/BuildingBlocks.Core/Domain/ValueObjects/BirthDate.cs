using BuildingBlocks.Core.Domain.Exceptions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record BirthDate
{
    // EF Core
    private BirthDate() { }

    public DateTime Value { get; private set; }


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

        return new BirthDate {Value = value};
    }

    public static implicit operator DateTime(BirthDate value) => value.Value;
}
