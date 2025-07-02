namespace BuildingBlocks.Resiliency.Options;

public class PolicyOptions
{
    public BulkheadPolicyOptions BulkheadPolicyOptions { get; set; } = new();
    public CircuitBreakerPolicyOptions CircuitBreakerPolicyOptions { get; set; } = new();
    public RetryPolicyOptions RetryPolicyOptions { get; set; } = new();
    public TimeoutPolicyOptions TimeoutPolicyOptions { get; set; } = new();
}
