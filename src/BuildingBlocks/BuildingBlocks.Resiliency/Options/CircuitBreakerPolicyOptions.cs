namespace BuildingBlocks.Resiliency.Options;

public class CircuitBreakerPolicyOptions
{
    /// <summary>
    ///  Gets or Sets an integer that specifies the duration of the "open" state, in seconds. When the circuit breaker is in the "open" state, all requests will fail fast without being executed, until the duration of the "open" state has elapsed.
    /// </summary>
    public int DurationOfBreak { get; set; } = 30;

    /// <summary>
    /// Gets or Sets an integer that specifies the number of exceptions that must occur within a specified period before the circuit breaker trips and enters the "open" state.
    /// </summary>
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 4;

    public int SamplingDuration { get; set; } = 60;
}
