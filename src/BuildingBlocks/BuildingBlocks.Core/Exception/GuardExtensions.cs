using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception.Types;

namespace BuildingBlocks.Core.Exception;

public static class GuardExtensions
{
    private static readonly Regex _regex = new(
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
        RegexOptions.Compiled);

    private static readonly HashSet<string> _allowedCurrency = new() { "USD", "EUR", };

    public static T Null<T>(this IGuardClause guardClause, T input, System.Exception exception)
    {
        if (input is null)
        {
            throw exception;
        }

        return input;
    }

    public static string NullOrEmpty(this IGuardClause guardClause, string input, System.Exception exception)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw exception;
        }

        return input;
    }

    public static string NullOrWhiteSpace(this IGuardClause guardClause, string input, System.Exception exception)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw exception;
        }

        return input;
    }

    public static decimal NegativeOrZero(this IGuardClause guardClause, decimal input, System.Exception exception)
    {
        return NegativeOrZero<decimal>(guardClause, input, exception);
    }

    public static int NegativeOrZero(this IGuardClause guardClause, int input, System.Exception exception)
    {
        return NegativeOrZero<int>(guardClause, input, exception);
    }

    public static long NegativeOrZero(this IGuardClause guardClause, long input, System.Exception exception)
    {
        return NegativeOrZero<long>(guardClause, input, exception);
    }

    public static double NegativeOrZero(this IGuardClause guardClause, double input, System.Exception exception)
    {
        return NegativeOrZero<double>(guardClause, input, exception);
    }

    private static T NegativeOrZero<T>(this IGuardClause guardClause, T input, System.Exception exception)
        where T : struct, IComparable
    {
        if (input.CompareTo(default(T)) <= 0)
        {
            throw exception;
        }

        return input;
    }

    public static decimal Negative(this IGuardClause guardClause, decimal input, System.Exception exception)
    {
        return Negative<decimal>(guardClause, input, exception);
    }

    public static int Negative(this IGuardClause guardClause, int input, System.Exception exception)
    {
        return Negative<int>(guardClause, input, exception);
    }

    public static long Negative(this IGuardClause guardClause, long input, System.Exception exception)
    {
        return Negative<long>(guardClause, input, exception);
    }

    public static double Negative(this IGuardClause guardClause, double input, System.Exception exception)
    {
        return Negative<double>(guardClause, input, exception);
    }

    private static T Negative<T>(this IGuardClause guardClause, T input, System.Exception exception)
        where T : struct, IComparable
    {
        if (input.CompareTo(default(T)) < 0)
        {
            throw exception;
        }

        return input;
    }

    public static bool NotExists(this IGuardClause guardClause, bool input, System.Exception exception)
    {
        if (input == false)
        {
            throw exception;
        }

        return input;
    }

    public static T NotFound<T>(this IGuardClause guardClause, T input, System.Exception exception)
    {
        if (input is null)
        {
            throw exception;
        }

        return input;
    }

    public static DateTime InvalidDate(this IGuardClause guardClause, DateTime date)
    {
        if (date == default(DateTime))
        {
            throw new InvalidDateException(date);
        }

        return date;
    }

    public static string InvalidEmail(this IGuardClause guardClause, string? email)
    {
        return guardClause.InvalidEmail(email, new InvalidEmailException(email ?? string.Empty));
    }

    public static string InvalidEmail(this IGuardClause guardClause, string? email, System.Exception exception)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw exception;
        }

        if (email.Length > 100)
        {
            throw exception;
        }

        email = email.ToLowerInvariant();
        if (!_regex.IsMatch(email))
        {
            throw exception;
        }

        return email;
    }

    public static string InvalidCurrency(this IGuardClause guardClause, string? currency)
    {
        return guardClause.InvalidCurrency(currency, new InvalidCurrencyException(currency));
    }

    public static string InvalidCurrency(this IGuardClause guardClause, string? currency, System.Exception exception)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw exception;
        }

        currency = currency.ToUpperInvariant();
        if (!_allowedCurrency.Contains(currency))
        {
            throw exception;
        }

        return currency;
    }

    public static string InvalidPhoneNumber(this IGuardClause guardClause, string? phoneNumber)
    {
        return guardClause.InvalidPhoneNumber(phoneNumber, new InvalidPhoneNumberException(phoneNumber));
    }

    public static string InvalidPhoneNumber(
        this IGuardClause guardClause,
        string? phoneNumber,
        System.Exception exception)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw exception;
        }

        if (phoneNumber.Length < 7)
        {
            throw exception;
        }

        if (phoneNumber.Length > 15)
        {
            throw exception;
        }

        return phoneNumber;
    }
}
