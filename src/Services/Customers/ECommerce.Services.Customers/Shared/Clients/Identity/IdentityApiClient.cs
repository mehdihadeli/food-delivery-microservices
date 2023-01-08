using System.Net.Http.Json;
using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Customers.Shared.Clients.Identity;

public class IdentityApiClient : IIdentityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IdentityApiClientOptions _options;

    public IdentityApiClient(HttpClient httpClient, IOptions<IdentityApiClientOptions> options)
    {
        _httpClient = Guard.Against.Null(httpClient, nameof(httpClient));
        _options = options.Value;
    }

    public async Task<GetUserByEmailResponse?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(email);
        Guard.Against.InvalidEmail(email);

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // https: //github.com/App-vNext/Polly#step-1--specify-the--exceptionsfaults-you-want-the-policy-to-handle
        var httpResponse = await _httpClient.GetAsync(
            $"/{_options.UsersEndpoint}/by-email/{email}",
            cancellationToken);

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // throw HttpResponseException instead of HttpRequestException (because we want detail response exception) with corresponding status code
        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();

        return await httpResponse.Content.ReadFromJsonAsync<GetUserByEmailResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CreateUserResponse?> CreateUserIdentityAsync(
        CreateUserRequest createUserRequest,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(createUserRequest);

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // https: //github.com/App-vNext/Polly#step-1--specify-the--exceptionsfaults-you-want-the-policy-to-handle
        var httpResponse = await _httpClient.PostAsJsonAsync(
            _options.UsersEndpoint,
            createUserRequest,
            cancellationToken);

        // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
        // throw HttpResponseException instead of HttpRequestException (because we want detail response exception) with corresponding status code
        await httpResponse.EnsureSuccessStatusCodeWithDetailAsync();

        return await httpResponse.Content.ReadFromJsonAsync<CreateUserResponse>(
            cancellationToken: cancellationToken);
    }
}
