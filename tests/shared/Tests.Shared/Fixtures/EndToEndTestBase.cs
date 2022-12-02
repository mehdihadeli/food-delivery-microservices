using System.Net.Http.Json;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.EntityFrameworkCore;
using Tests.Shared.Auth;

namespace Tests.Shared.Fixtures;

[Trait("Category", "EndToEnd")]
public class EndToEndTestTestBase<TEntryPoint, TWContext, TRContext> :
    IntegrationTestBase<TEntryPoint, TWContext, TRContext>
    where TWContext : DbContext
    where TRContext : MongoDbContext
    where TEntryPoint : class
{
    public EndToEndTestTestBase(
        SharedFixture<TEntryPoint, TWContext, TRContext> sharedFixture,
        ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper)
    {
    }

    protected virtual UserType UserType => UserType.Admin;

    public async Task<TResponse?> GetAsync<TResponse>(string requestUrl, CancellationToken cancellationToken = default)
    {
        var client = GetClient(UserType);
        return await client.GetFromJsonAsync<TResponse>(requestUrl, cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUrl, TRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = GetClient(UserType);
        return await client.PostAsJsonAsync<TRequest, TResponse>(requestUrl, request, cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string requestUrl,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = GetClient(UserType);
        return await client.PutAsJsonAsync<TRequest, TResponse>(requestUrl, request, cancellationToken);
    }

    public async Task Delete(string requestUrl, CancellationToken cancellationToken = default)
    {
        var client = GetClient(UserType);
        await client.DeleteAsync(requestUrl, cancellationToken);
    }

    private HttpClient GetClient(UserType userType)
    {
        switch (userType)
        {
            case UserType.Admin:
                return Fixture.AdminHttpClient;
            case UserType.User:
                return Fixture.NormalUserHttpClient;
            default:
                return Fixture.GuestClient;
        }
    }
}
