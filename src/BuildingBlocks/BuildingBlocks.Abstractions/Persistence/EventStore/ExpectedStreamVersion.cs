namespace BuildingBlocks.Abstractions.Persistence.EventStore;

public record ExpectedStreamVersion(long Value)
{
    public static readonly ExpectedStreamVersion NoStream = new(-1);
    public static readonly ExpectedStreamVersion Any = new(-2);
}

public record StreamReadPosition(long Value)
{
    public static readonly StreamReadPosition Start = new(0L);
}

public record StreamTruncatePosition(long Value);
