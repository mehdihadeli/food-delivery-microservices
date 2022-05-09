namespace BuildingBlocks.Abstractions.Messaging;

public abstract class Acknowledgement
{
}

public class Ack : Acknowledgement
{
}

public class Nack : Acknowledgement
{
    public Nack(Exception exception, bool requeue = true)
    {
        Requeue = requeue;
        Exception = exception;
    }

    public bool Requeue { get; }
    public Exception Exception { get; }
}

public class Reject : Acknowledgement
{
    public bool Requeue { get; }

    public Reject(bool requeue = true)
    {
        Requeue = requeue;
    }
}
