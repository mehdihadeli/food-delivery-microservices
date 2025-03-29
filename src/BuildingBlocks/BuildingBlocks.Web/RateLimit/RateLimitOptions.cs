namespace BuildingBlocks.Web.RateLimit;

public class RateLimitOptions
{
    public int Limit { get; set; }
    public double PeriodInMs { get; set; }
    public int QueueLimit { get; set; }
}
