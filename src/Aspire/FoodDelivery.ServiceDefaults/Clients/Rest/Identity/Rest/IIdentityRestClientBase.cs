using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extensions;
using FoodDelivery.ServiceDefaults.Clients.Rest.Identity.Dtos;
using Microsoft.Extensions.Options;
using Polly.Wrap;

namespace FoodDelivery.ServiceDefaults.Clients.Rest.Identity.Rest;

public class IdentityRestClient(
    HttpClient httpClient,
    IOptions<IdentityRestClientOptions> options,
    AsyncPolicyWrap combinedPolicy
) : IIdentityRestClient
{
    private readonly IdentityRestClientOptions _options = options.Value;

    public async Task<GetUserByEmailClientResponseDto?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        email.NotBeNullOrWhiteSpace();
        email.NotBeInvalidEmail();

        try
        {
            var response = await combinedPolicy.ExecuteAsync(async () =>
            {
                // EnsureStatusCode throw HttpRequestException when we have 404 status code as well, we should handle it
                var response = await httpClient.GetFromJsonAsync<GetUserByEmailClientResponseDto>(
                    $"{_options.GetUserByEmailEndpoint}/{email}",
                    cancellationToken
                );

                return response;
            });

            return response;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException($"user with id '{email}' not found");
        }
    }

    public async Task<CreateUserIdentityClientResponseDto?> CreateUserIdentityAsync(
        CreateUserClientRequestDto createUserClientRequestDto,
        CancellationToken cancellationToken = default
    )
    {
        createUserClientRequestDto.NotBeNull();

        var response = await combinedPolicy.ExecuteAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync(
                _options.CreateUserEndpoint,
                createUserClientRequestDto,
                cancellationToken
            );

            // https://stackoverflow.com/questions/21097730/usage-of-ensuresuccessstatuscode-and-handling-of-httprequestexception-it-throws
            // throw HttpResponseException instead of HttpRequestException (because we want detail response exception) with corresponding status code
            await response.EnsureSuccessStatusCodeWithDetailAsync();

            var result = await response.Content.ReadFromJsonAsync<CreateUserIdentityClientResponseDto>(
                cancellationToken: cancellationToken
            );

            return result;
        });

        return response;
    }
}
