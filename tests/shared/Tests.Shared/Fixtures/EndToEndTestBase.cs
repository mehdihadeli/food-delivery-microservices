using System.Net.Http.Json;
using BuildingBlocks.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Tests.Shared.Mocks;
using Xunit.Abstractions;

namespace Tests.Shared.Fixtures;

public class EndToEndTestBase<TEntryPoint> : IntegrationTestBase<TEntryPoint>
    where TEntryPoint : class
{
    protected EndToEndTestBase(IntegrationTestFixture<TEntryPoint> integrationTestFixture,
        ITestOutputHelper outputHelper)
        : base(integrationTestFixture, outputHelper)
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
                return AdminClient;
            case UserType.User:
                return UserClient;
            default:
                return GuestClient;
        }
    }
}

public class EndToEndTestBase<TEntryPoint, TDbContext> : EndToEndTestBase<TEntryPoint>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    public EndToEndTestBase(IntegrationTestFixture<TEntryPoint, TDbContext> integrationTestFixture,
        ITestOutputHelper outputHelper)
        : base(integrationTestFixture, outputHelper)
    {
    }
}
