using System.Net.Http.Json;
using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
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
        Guard.Against.NullOrEmpty(email, nameof(email));
        Guard.Against.InvalidEmail(email);

        var userIdentity = await _httpClient.GetFromJsonAsync<GetUserByEmailResponse>(
            $"/{_options.UsersEndpoint}/by-email/{email}",
            cancellationToken);

        return userIdentity;
    }

    public async Task<CreateUserResponse?> CreateUserIdentityAsync(
        CreateUserRequest createUserRequest,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(createUserRequest, nameof(createUserRequest));

        var response = await _httpClient.PostAsJsonAsync(
            _options.UsersEndpoint,
            createUserRequest,
            cancellationToken);

        // throws if not 200-299
        response.EnsureSuccessStatusCode();

        var createdUser =
            await response.Content.ReadFromJsonAsync<CreateUserResponse?>(cancellationToken: cancellationToken);

        return createdUser;
    }
}
