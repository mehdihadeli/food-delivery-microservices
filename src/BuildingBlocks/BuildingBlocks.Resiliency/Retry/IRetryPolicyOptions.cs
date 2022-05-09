namespace BuildingBlocks.Resiliency.Retry;

public interface IRetryPolicyOptions
{
    int RetryCount { get; set; }
}
