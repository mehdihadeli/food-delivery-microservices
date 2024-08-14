using System.Net.Http.Json;
using AutoMapper;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Resiliency;
using BuildingBlocks.Web.Extensions;
using FoodDelivery.Services.Customers.Shared.Clients.Identity.Dtos;
using FoodDelivery.Services.Customers.Users.Model;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace FoodDelivery.Services.Customers.Shared.Clients.Identity;

public class IdentityApiClient : IIdentityApiClient
{
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;
    private readonly IdentityApiClientOptions _options;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _combinedPolicy;

    public IdentityApiClient(
        HttpClient httpClient,
        IMapper mapper,
        IOptions<IdentityApiClientOptions> options,
        IOptions<PolicyOptions> policyOptions
    )
    {
        _mapper = mapper;
        _httpClient = httpClient.NotBeNull();
        _options = options.Value;

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(policyOptions.Value.RetryCount);

        var timeoutPolicy = Policy.TimeoutAsync(policyOptions.Value.TimeOutDuration, TimeoutStrategy.Pessimistic);

        // at any given time there will 3 parallel requests execution for specific service call and another 6 requests for other services can be in the queue. So that if the response from customer service is delayed or blocked then we don’t use too many resources
        var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(3, 6);

        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                policyOptions.Value.RetryCount + 1,
                TimeSpan.FromSeconds(policyOptions.Value.BreakDuration)
            );

        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, bulkheadPolicy);

        _combinedPolicy = combinedPolicy.WrapAsync(timeoutPolicy);
    }

    public async Task<UserIdentity?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        email.NotBeNullOrWhiteSpace();
        email.NotBeInvalidEmail();

        var httpResponse = await _combinedPolicy.ExecuteAsync(async () =>
        {
            // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
            // https: //github.com/App-vNext/Polly#step-1--specify-the--exceptionsfaults-you-want-the-policy-to-handle
            var httpResponse = await _httpClient.GetAsync(
                $"/{_options.UsersEndpoint}/by-email/{email}",
                cancellationToken
            );
            return httpResponse;
        });

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // throw HttpResponseException instead of HttpRequestException (because we want detail response exception) with corresponding status code
        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();

        var userDto = await httpResponse.Content.ReadFromJsonAsync<GetUserByEmailClientDto>(
            cancellationToken: cancellationToken
        );
        var user = _mapper.Map<UserIdentity>(userDto?.UserIdentity);

        return user;
    }

    public async Task<UserIdentity?> CreateUserIdentityAsync(
        CreateUserClientDto createUserClientDto,
        CancellationToken cancellationToken = default
    )
    {
        createUserClientDto.NotBeNull();

        var httpResponse = await _combinedPolicy.ExecuteAsync(async () =>
        {
            // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
            // https: //github.com/App-vNext/Polly#step-1--specify-the--exceptionsfaults-you-want-the-policy-to-handle
            var httpResponse = await _httpClient.PostAsJsonAsync(
                _options.UsersEndpoint,
                createUserClientDto,
                cancellationToken
            );
            return httpResponse;
        });

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // throw HttpResponseException instead of HttpRequestException (because we want detail response exception) with corresponding status code
        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();

        var userDto = await httpResponse.Content.ReadFromJsonAsync<CreateUserIdentityClientDto>(
            cancellationToken: cancellationToken
        );
        var user = _mapper.Map<UserIdentity>(userDto?.UserIdentity);

        return user;
    }
}
