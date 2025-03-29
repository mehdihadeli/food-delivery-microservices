using System.Net;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Resiliency.Options;
using Microsoft.AspNetCore.Builder;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace BuildingBlocks.Core.Resiliency;

internal static class DependencyInjectionExtensions
{
    internal static WebApplicationBuilder AddCoreResiliency(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<PolicyOptions>(nameof(PolicyOptions));
        var policyOptions = builder.Configuration.BindOptions<PolicyOptions>();

        // `AsyncPolicyWrap<HttpResponseMessage>` can be injected in clients and can be reused.
        builder.Services.AddSingleton<AsyncPolicyWrap<HttpResponseMessage>>(sp =>
        {
            // Retry policy: Do not retry on 404 Not Found
            var retryPolicy = Policy
                .Handle<HttpRequestException>(ex => ex.StatusCode != HttpStatusCode.NotFound) // Exclude 404
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.NotFound) // Exclude 404
                .RetryAsync(policyOptions.RetryPolicyOptions.Count);

            // HttpClient itself will still enforce its own timeout, which is 100 seconds by default. To fix this issue, you need to set the HttpClient.Timeout property to match or exceed the timeout configured in Polly's policy.
            var timeoutPolicy = Policy.TimeoutAsync(
                policyOptions.TimeoutPolicyOptions.TimeoutInSeconds,
                TimeoutStrategy.Pessimistic
            );

            // at any given time there will 3 parallel requests execution for specific service call and another 6 requests for other services can be in the queue. So that if the response from customer service is delayed or blocked then we don’t use too many resources
            var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(3, 6);

            // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    policyOptions.RetryPolicyOptions.Count + 1,
                    TimeSpan.FromSeconds(policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak)
                );

            var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, bulkheadPolicy);

            var finalPolicy = combinedPolicy.WrapAsync(timeoutPolicy);

            return finalPolicy;
        });

        builder.Services.AddSingleton<AsyncPolicyWrap>(sp =>
        {
            // Retry policy
            var retryPolicy = Policy
                .Handle<HttpRequestException>(ex => ex.StatusCode != HttpStatusCode.NotFound) // Exclude 404
                .RetryAsync(policyOptions.RetryPolicyOptions.Count);

            // Timeout policy
            var timeoutPolicy = Policy.TimeoutAsync(
                policyOptions.TimeoutPolicyOptions.TimeoutInSeconds,
                TimeoutStrategy.Pessimistic
            );

            // Bulkhead policy
            var bulkheadPolicy = Policy.BulkheadAsync(3, 6);

            // Circuit breaker policy
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    policyOptions.CircuitBreakerPolicyOptions.ExceptionsAllowedBeforeBreaking,
                    TimeSpan.FromSeconds(policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak)
                );

            // Combine policies
            // Combine policies
            var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, bulkheadPolicy);

            var finalPolicy = combinedPolicy.WrapAsync(timeoutPolicy);

            return finalPolicy;
        });

        return builder;
    }
}
