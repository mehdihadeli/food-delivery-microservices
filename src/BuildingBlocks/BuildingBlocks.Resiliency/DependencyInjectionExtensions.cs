using System.Net;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Resiliency.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace BuildingBlocks.Resiliency;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomResiliency(
        this IHostApplicationBuilder builder,
        bool globalHttpClientResiliency = true
    )
    {
        builder.Services.AddServiceDiscovery();

        AddResiliencyCore(builder);

        if (globalHttpClientResiliency)
        {
            // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
            // set resiliency globally on clients
            builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
            {
                // Turn on resilience by default
                httpClientBuilder
                    .AddStandardResilienceHandler()
                    .Configure(
                        (cfg, sp) =>
                        {
                            // resiliency using `Microsoft.Extensions.Http.Resilience`
                            // https://learn.microsoft.com/en-us/dotnet/core/resilience/
                            // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
                            var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value.NotBeNull();

                            cfg.AttemptTimeout = new HttpTimeoutStrategyOptions
                            {
                                Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds),
                            };

                            cfg.TotalRequestTimeout = new HttpTimeoutStrategyOptions
                            {
                                Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds),
                            };

                            cfg.Retry = new HttpRetryStrategyOptions
                            {
                                BackoffType = DelayBackoffType.Exponential,
                                MaxRetryAttempts = policyOptions.RetryPolicyOptions.Count,
                                UseJitter = true,
                            };

                            cfg.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                            {
                                SamplingDuration = TimeSpan.FromSeconds(
                                    policyOptions.CircuitBreakerPolicyOptions.SamplingDuration
                                ), // Ensure this is >= 2 * TimeoutInSeconds
                                BreakDuration = TimeSpan.FromSeconds(
                                    policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak
                                ),
                                MinimumThroughput = policyOptions
                                    .CircuitBreakerPolicyOptions
                                    .ExceptionsAllowedBeforeBreaking,
                                ShouldHandle = static args =>
                                    ValueTask.FromResult(
                                        args
                                            is {
                                                Outcome.Result.StatusCode: HttpStatusCode.RequestTimeout
                                                    or HttpStatusCode.TooManyRequests
                                            }
                                    ),
                            };
                        }
                    );

                // // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli#add-custom-resilience-handlers
                // httpClientBuilder.AddResilienceHandler(
                //     "CustomPipeline",
                //     (pipelineBuilder, ctx) =>
                //     {
                //         var policyOptions = ctx
                //             .ServiceProvider.GetRequiredService<IOptions<PolicyOptions>>()
                //             .Value.NotBeNull();
                //
                //         // See: https://www.pollydocs.org/strategies/retry.html
                //         pipelineBuilder.AddRetry(
                //             new HttpRetryStrategyOptions
                //             {
                //                 BackoffType = DelayBackoffType.Exponential,
                //                 MaxRetryAttempts = policyOptions.RetryPolicyOptions.Count,
                //                 UseJitter = true,
                //             }
                //         );
                //
                //         // See: https://www.pollydocs.org/strategies/circuit-breaker.html
                //         pipelineBuilder.AddCircuitBreaker(
                //             new HttpCircuitBreakerStrategyOptions
                //             {
                //                 BreakDuration = TimeSpan.FromSeconds(
                //                     policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak
                //                 ),
                //                 SamplingDuration = TimeSpan.FromSeconds(
                //                     policyOptions.CircuitBreakerPolicyOptions.SamplingDuration
                //                 ),
                //                 FailureRatio = 0.2,
                //                 MinimumThroughput = policyOptions
                //                     .CircuitBreakerPolicyOptions
                //                     .ExceptionsAllowedBeforeBreaking,
                //                 ShouldHandle = static args =>
                //                 {
                //                     return ValueTask.FromResult(
                //                         args
                //                             is {
                //                                 Outcome.Result.StatusCode: HttpStatusCode.RequestTimeout
                //                                     or HttpStatusCode.TooManyRequests
                //                             }
                //                     );
                //                 },
                //             }
                //         );
                //
                //         // See: https://www.pollydocs.org/strategies/timeout.html
                //         pipelineBuilder.AddTimeout(
                //             TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds)
                //         );
                //     }
                // );
            });
        }

        // https://learn.microsoft.com/en-us/dotnet/core/resilience/?tabs=dotnet-cli#build-a-resilience-pipeline
        builder.Services.AddResiliencePipeline(
            "shared-resilience-pipeline",
            static (pipelineBuilder, ctx) =>
            {
                var policyOptions = ctx.ServiceProvider.GetRequiredService<IOptions<PolicyOptions>>().Value.NotBeNull();

                // See: https://www.pollydocs.org/strategies/retry.html
                pipelineBuilder.AddRetry(
                    new RetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        MaxRetryAttempts = policyOptions.RetryPolicyOptions.Count,
                        UseJitter = true,
                    }
                );

                // See: https://www.pollydocs.org/strategies/circuit-breaker.html
                pipelineBuilder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions
                    {
                        BreakDuration = TimeSpan.FromSeconds(policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak),
                        SamplingDuration = TimeSpan.FromSeconds(
                            policyOptions.CircuitBreakerPolicyOptions.SamplingDuration
                        ),
                        FailureRatio = 0.2,
                        MinimumThroughput = policyOptions.CircuitBreakerPolicyOptions.ExceptionsAllowedBeforeBreaking,
                    }
                );

                // See: https://www.pollydocs.org/strategies/timeout.html
                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds));
            }
        );

        return builder;
    }

    public static IHttpClientBuilder ConfigureStandardResilienceHandler(this IHttpClientBuilder httpClientBuilder)
    {
        httpClientBuilder
            .AddStandardResilienceHandler()
            .Configure(
                (cfg, sp) =>
                {
                    // resiliency using `Microsoft.Extensions.Http.Resilience`
                    // https://learn.microsoft.com/en-us/dotnet/core/resilience/
                    // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
                    var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>().Value.NotBeNull();

                    cfg.AttemptTimeout = new HttpTimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds),
                    };

                    cfg.TotalRequestTimeout = new HttpTimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds),
                    };

                    cfg.Retry = new HttpRetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        MaxRetryAttempts = policyOptions.RetryPolicyOptions.Count,
                        UseJitter = true,
                    };

                    cfg.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                    {
                        SamplingDuration = TimeSpan.FromSeconds(
                            policyOptions.CircuitBreakerPolicyOptions.SamplingDuration
                        ), // Ensure this is >= 2 * TimeoutInSeconds
                        BreakDuration = TimeSpan.FromSeconds(policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak),
                        MinimumThroughput = policyOptions.CircuitBreakerPolicyOptions.ExceptionsAllowedBeforeBreaking,
                        ShouldHandle = static args =>
                        {
                            return ValueTask.FromResult(
                                args
                                    is {
                                        Outcome.Result.StatusCode: HttpStatusCode.RequestTimeout
                                            or HttpStatusCode.TooManyRequests
                                    }
                            );
                        },
                    };
                }
            );

        return httpClientBuilder;
    }

    public static IHttpClientBuilder ConfigureDefaultResilienceHandler(this IHttpClientBuilder httpClientBuilder)
    {
        // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli#add-custom-resilience-handlers
        httpClientBuilder.AddResilienceHandler(
            "CustomPipeline",
            (pipelineBuilder, ctx) =>
            {
                var policyOptions = ctx.ServiceProvider.GetRequiredService<IOptions<PolicyOptions>>().Value.NotBeNull();

                // See: https://www.pollydocs.org/strategies/retry.html
                pipelineBuilder.AddRetry(
                    new HttpRetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        MaxRetryAttempts = policyOptions.RetryPolicyOptions.Count,
                        UseJitter = true,
                    }
                );

                // See: https://www.pollydocs.org/strategies/circuit-breaker.html
                pipelineBuilder.AddCircuitBreaker(
                    new HttpCircuitBreakerStrategyOptions
                    {
                        BreakDuration = TimeSpan.FromSeconds(policyOptions.CircuitBreakerPolicyOptions.DurationOfBreak),
                        SamplingDuration = TimeSpan.FromSeconds(
                            policyOptions.CircuitBreakerPolicyOptions.SamplingDuration
                        ),
                        FailureRatio = 0.2,
                        MinimumThroughput = policyOptions.CircuitBreakerPolicyOptions.ExceptionsAllowedBeforeBreaking,
                        ShouldHandle = static args =>
                        {
                            return ValueTask.FromResult(
                                args
                                    is {
                                        Outcome.Result.StatusCode: HttpStatusCode.RequestTimeout
                                            or HttpStatusCode.TooManyRequests
                                    }
                            );
                        },
                    }
                );

                // See: https://www.pollydocs.org/strategies/timeout.html
                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(policyOptions.TimeoutPolicyOptions.TimeoutInSeconds));
            }
        );

        return httpClientBuilder;
    }

    private static void AddResiliencyCore(IHostApplicationBuilder builder)
    {
        builder.Services.AddValidationOptions<PolicyOptions>();
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
    }
}
