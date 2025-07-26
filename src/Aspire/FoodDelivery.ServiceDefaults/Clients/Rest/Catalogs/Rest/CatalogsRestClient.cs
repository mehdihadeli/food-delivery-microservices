using System.Globalization;
using System.Net.Http.Json;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Core.Exception;
using FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs.Dtos;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Polly.Wrap;

namespace FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs.Rest;

public class CatalogsRestClient(
    HttpClient httpClient,
    IOptions<CatalogsRestClientOptions> options,
    AsyncPolicyWrap combinedPolicy
) : ICatalogsRestClient
{
    private readonly CatalogsRestClientOptions _options = options.Value;

    public async Task<GetProductByPageClientResponseDto> GetProductByPageAsync(
        GetProductsByPageClientRequestDto getProductsByPageClientRequestDto,
        CancellationToken cancellationToken
    )
    {
        // https://stackoverflow.com/a/67877742/581476
        var qb = new QueryBuilder
        {
            { "limit", getProductsByPageClientRequestDto.PageSize.ToString(CultureInfo.InvariantCulture) },
            { "skip", getProductsByPageClientRequestDto.PageNumber.ToString(CultureInfo.InvariantCulture) },
        };

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var getProductsByPageResponse = await combinedPolicy.ExecuteAsync(async () =>
        {
            // https://ollama.com/blog/openai-compatibility
            // https://www.youtube.com/watch?v=38jlvmBdBrU
            // https://platform.openai.com/docs/api-reference/chat/create
            // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
            var response = await httpClient.GetFromJsonAsync<GetProductByPageClientResponseDto>(
                $"{_options.GetProductByPageEndpoint}?{qb.ToQueryString().Value}",
                cancellationToken
            );

            return response;
        });

        if (
            getProductsByPageResponse is null
            || getProductsByPageResponse.Products is null
            || getProductsByPageResponse.Products.Items is null
        )
        {
            throw new Exception("products page list cannot be null");
        }

        return getProductsByPageResponse;
    }

    public async Task<GetProductByIdClientResponseDto?> GetProductByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await combinedPolicy.ExecuteAsync(async () =>
        {
            var response = await httpClient.GetFromJsonAsync<GetProductByIdClientResponseDto>(
                $"{_options.GetProductByIdEndpoint}/{id}",
                cancellationToken
            );

            return response;
        });

        return response;
    }
}
