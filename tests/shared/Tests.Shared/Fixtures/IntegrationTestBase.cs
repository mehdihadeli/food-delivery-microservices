using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using BuildingBlocks.Core.Extensions;
using Microsoft.Extensions.Logging;
using Tests.Shared.Builders;
using Tests.Shared.Mocks;

namespace Tests.Shared.Fixtures;

public abstract class IntegrationTestBase<TEntryPoint, TDbContext> : IntegrationTestBase<TEntryPoint>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint, TDbContext> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
    }
}

public abstract class IntegrationTestBase<TEntryPoint> : IClassFixture<IntegrationTestFixture<TEntryPoint>>
    where TEntryPoint : class
{
    protected CancellationTokenSource CancellationTokenSource { get; } = new(TimeSpan.FromSeconds(10));
    protected IServiceScope Scope { get; }
    protected IntegrationTestFixture<TEntryPoint> IntegrationTestFixture { get; }

    protected ILogger<IntegrationTestBase<TEntryPoint>> Logger =>
        Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase<TEntryPoint>>>();

    protected CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected TextWriter TextWriter => Scope.ServiceProvider.GetRequiredService<TextWriter>();

    protected HttpClient AdminClient { get; }
    protected HttpClient GuestClient { get; }
    protected HttpClient UserClient { get; }

    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint> integrationTestFixture,
        ITestOutputHelper outputHelper)
    {
        IntegrationTestFixture = integrationTestFixture;
        Scope = integrationTestFixture.ServiceProvider.CreateScope();
        integrationTestFixture.SetOutputHelper(outputHelper);

        AdminClient = integrationTestFixture.CreateNewClient(services =>
        {
        });

        GuestClient = integrationTestFixture.CreateNewClient(services =>
        {
        });

        UserClient = integrationTestFixture.CreateNewClient(services =>
        {
        });

        var admin = new LoginRequestBuilder().Build();
        var user = new LoginRequestBuilder()
            .WithUserNameOrEmail(Constants.Users.User.UserName)
            .WithPassword(Constants.Users.User.Password)
            .Build();

        var adminLoginResult =
            GuestClient.PostAsJsonAsync<LoginUserRequestMock, LoginResponseMock>(Constants.LoginApi, admin)
                .GetAwaiter().GetResult();

        var userLoginResult =
            GuestClient.PostAsJsonAsync<LoginUserRequestMock, LoginResponseMock>(Constants.LoginApi, user)
                .GetAwaiter().GetResult();

        AdminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminLoginResult?.AccessToken);

        UserClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userLoginResult?.AccessToken);
    }
}
