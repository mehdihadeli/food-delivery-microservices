using BuildingBlocks.Core.Domain.Exceptions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record BirthDate
{
    public DateTime Value { get; private set; }

    public static BirthDate? Null => null;

    public static BirthDate Create(DateTime value)
    {
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

    public static implicit operator BirthDate?(DateTime? value) => value == null ? null : Create((DateTime)value);

    public static implicit operator DateTime?(BirthDate? value) => value?.Value ?? null;
}
