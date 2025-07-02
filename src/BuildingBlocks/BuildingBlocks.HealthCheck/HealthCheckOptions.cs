namespace BuildingBlocks.HealthCheck;

public class HealthCheckOptions
{
    public long RequestTimeoutSecond { get; set; } = 5;
    public long ExpireAfterSecond { get; set; } = 5;
}
