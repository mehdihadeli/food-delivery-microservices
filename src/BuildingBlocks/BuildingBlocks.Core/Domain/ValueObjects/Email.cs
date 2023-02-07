using FluentValidation;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Email
{
    // EF
    private Email(string value)
    {
        Value = value;
    }

    // Note: in entities with none default constructor, for setting constructor parameter, we need a private set property
    // when we didn't define this property in fluent configuration mapping (if so we can remove private set) , because for getting mapping list of properties to set
    // in the constructor it should not be read only without set (for bypassing calculate fields)- https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
    public string Value { get; private set; }

    public static Email Of(string value)
    {
        // validations should be placed here instead of constructor
        new EmailValidator().ValidateAndThrow(value);
        return new Email(value);
    }

    public static implicit operator string(Email value) => value.Value;

    private sealed class EmailValidator : AbstractValidator<string>
    {
        public EmailValidator()
        {
            RuleFor(email => email).NotEmpty();
            RuleFor(email => email).EmailAddress();
        }
    }
}
