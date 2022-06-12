namespace Tests.Shared.Probing;

public class AssertErrorException : Exception
{
    public AssertErrorException(string message)
        : base(message)
    {
    }
}
