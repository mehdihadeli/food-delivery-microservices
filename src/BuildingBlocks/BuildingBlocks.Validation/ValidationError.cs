namespace BuildingBlocks.Validation;

public class ValidationError(string field, string message)
{
    public string? Field { get; } = field != string.Empty ? field : null;

    public string Message { get; } = message;
}
