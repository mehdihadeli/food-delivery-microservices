using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Resiliency.Retry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Resiliency.Extensions;

public static partial class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddRetryPolicyHandler(
        this IHttpClientBuilder httpClientBuilder)
    {
        // https://stackoverflow.com/questions/53604295/logging-polly-wait-and-retry-policy-asp-net-core-2-1
        return httpClientBuilder.AddPolicyHandler((sp, _) =>
        {
            var options = sp.GetRequiredService<IConfiguration>().GetOptions<PolicyOptions>(nameof(PolicyOptions));

            Guard.Against.Null(options, nameof(options));

            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var retryLogger = loggerFactory.CreateLogger("PollyHttpRetryPoliciesLogger");

            return HttpRetryPolicies.GetHttpRetryPolicy(retryLogger, options);
        });
    }
}
