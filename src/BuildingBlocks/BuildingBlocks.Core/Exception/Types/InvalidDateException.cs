namespace BuildingBlocks.Core.Exception.Types;

public class InvalidDateException(DateTime date) : BadRequestException($"Date: '{date}' is invalid.")
{
    public DateTime Date { get; } = date;
}
