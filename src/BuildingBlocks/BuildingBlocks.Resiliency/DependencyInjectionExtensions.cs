using System.Net;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Resiliency.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace BuildingBlocks.Resiliency;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddCustomResiliency(
        this WebApplicationBuilder builder,
        bool globalHttpClientResiliency = true
    )
    {
        if (globalHttpClientResiliency)
        {
            // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
            // set resiliency globally on clients
            builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
            {
                // add a default ResilienceHandler with `AddResilienceHandler` "standard" as name of handler
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
}
