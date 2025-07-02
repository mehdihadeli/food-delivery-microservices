using System.Net;
using System.Threading.RateLimiting;
using BuildingBlocks.Core.Extensions;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.RateLimit;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomRateLimit(this IHostApplicationBuilder builder)
    {
        var rateLimitingOptions = builder.Configuration.BindOptions<RateLimitOptions>(nameof(RateLimitOptions));

        builder.Services.AddRateLimiter(opt =>
        {
            // all individual policies should have their own OnRejected, I guess this is fallback only ??? don't know at the moment.
            opt.OnRejected = (OnRejectedContext context, CancellationToken _) =>
            {
                var problemDetailsService =
                    context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.TooManyRequests,
                    Title = HttpStatusCode.TooManyRequests.Humanize(),
                };

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                return problemDetailsService.WriteAsync(
                    new ProblemDetailsContext { HttpContext = context.HttpContext, ProblemDetails = problemDetails }
                );
            };

            opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.GetClientIp() ?? "N/A",
                    factory: _ =>
                    {
                        var permitLimit = rateLimitingOptions.Limit;
                        var periodInMs = rateLimitingOptions.PeriodInMs;

                        return new FixedWindowRateLimiterOptions()
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromMilliseconds(periodInMs),
                            QueueLimit = rateLimitingOptions.QueueLimit,
                        };
                    }
                )
            );
        });

        return builder;
    }

    private static string? GetClientIp(this HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0]; // Take the first IP in the list
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
