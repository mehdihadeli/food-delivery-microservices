namespace Tests.Shared.Probing;

// Ref: https://github.com/kgrzybek/modular-monolith-with-ddd/
public interface IProbe
{
    bool IsSatisfied();

    Task SampleAsync();

    string DescribeFailureTo();
}

public interface IProbe<T>
{
    bool IsSatisfied(T sample);

    Task<T> GetSampleAsync();

    string DescribeFailureTo();
}
