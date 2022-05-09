using BuildingBlocks.Resiliency.Retry;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace BuildingBlocks.Resiliency.CircuitBreaker;

// Ref: https://anthonygiretti.com/2019/03/26/best-practices-with-httpclient-and-retry-policies-with-polly-in-net-core-2-part-2/
public static class HttpCircuitBreakerPolicies
{
    public static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetHttpCircuitBreakerPolicy(
        ILogger logger,
        ICircuitBreakerPolicyOptions circuitBreakerPolicyConfig)
    {
        return HttpPolicyBuilders.GetBaseBuilder()
            .CircuitBreakerAsync(
                circuitBreakerPolicyConfig.RetryCount + 1,
                TimeSpan.FromSeconds(circuitBreakerPolicyConfig.BreakDuration),
                (result, breakDuration) =>
                {
                    OnHttpBreak(result, breakDuration, circuitBreakerPolicyConfig.RetryCount, logger);
                },
                () => { OnHttpReset(logger); });
    }

    private static void OnHttpBreak(
        DelegateResult<HttpResponseMessage> result,
        TimeSpan breakDuration,
        int retryCount,
        ILogger logger)
    {
        logger.LogWarning(
            "Service shutdown during {BreakDuration} after {DefaultRetryCount} failed retries",
            breakDuration,
            retryCount);
        throw new BrokenCircuitException("Service inoperative. Please try again later");
    }

    public static void OnHttpReset(ILogger logger)
    {
        logger.LogInformation("Service restarted");
    }
}
