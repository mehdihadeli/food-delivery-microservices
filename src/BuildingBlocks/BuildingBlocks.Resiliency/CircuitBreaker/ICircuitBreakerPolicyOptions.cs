namespace BuildingBlocks.Resiliency.CircuitBreaker;

public interface ICircuitBreakerPolicyOptions
{
    int RetryCount { get; set; }
    int BreakDuration { get; set; }
}
