using System.Security.Claims;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Persistence.Mongo;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Mocks;
using Tests.Shared.Probing;
using Xunit.Abstractions;
using IBus = BuildingBlocks.Abstractions.Messaging.IBus;

namespace Tests.Shared.Fixtures;

// Ref: https://github.com/jbogard/ContosoUniversityDotNetCore-Pages/blob/master/ContosoUniversity.IntegrationTests/SliceFixture.cs
public class IntegrationTestFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly CustomWebApplicationFactory<TEntryPoint> _factory;

    public IntegrationTestFixture()
    {
        // Ref: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#basic-tests-with-the-default-webapplicationfactory
        _factory = new CustomWebApplicationFactory<TEntryPoint>();
    }

    public int Timeout { get; set; } = 180;

    public IServiceProvider ServiceProvider => _factory.Services;
    public IConfiguration Configuration => _factory.Configuration;

    // We could use `WithWebHostBuilder` method for specific config on own factory host
    public CustomWebApplicationFactory<TEntryPoint> Factory => _factory;

    // TestHarness will register as singleton
    public ITestHarness Harness => ServiceProvider.GetRequiredService<ITestHarness>();

    public void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        // var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        // loggerFactory.AddXUnit(outputHelper);
        _factory.OutputHelper = outputHelper;
    }

    public IHttpContextAccessor HttpContextAccessor =>
        ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    public HttpClient CreateClient() => _factory.CreateClient();

    public void RegisterTestServices(Action<IServiceCollection> services) =>
        _factory.TestRegistrationServices = services;

    public async Task AssertEventually(IProbe probe, int timeout)
    {
        await new Poller(timeout).CheckAsync(probe);
    }

    public async ValueTask ExecuteScopeAsync(Func<IServiceProvider, ValueTask> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async ValueTask<T> ExecuteScopeAsync<T>(Func<IServiceProvider, ValueTask<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public MockAuthUser CreateAdminUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.Admin.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.Admin.UserId),
            new(ClaimTypes.Name, Constants.Users.Admin.UserName),
            new(ClaimTypes.Email, Constants.Users.Admin.Email)
        };

        return _ = new MockAuthUser(otherClaims.Concat(new[] {roleClaim}).ToArray());
    }

    public MockAuthUser CreateNormalUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.User.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.User.UserId),
            new(ClaimTypes.Name, Constants.Users.User.UserName),
            new(ClaimTypes.Email, Constants.Users.User.Email)
        };

        return _ = new MockAuthUser(otherClaims.Concat(new[] {roleClaim}).ToArray());
    }

    public void SetUserRole(string role, string? sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim> {new Claim(ClaimTypes.Role, role), new(ClaimTypes.Name, sub)};

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(_ => claimsPrincipal);

        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public void SetUserRoles(string[] roles, string? sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        claims.Add(new Claim(ClaimTypes.Name, sub));

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(_ => claimsPrincipal);

        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return await mediator.Send(request);
        });
    }

    public async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task SendAsync<T>(T request, CancellationToken cancellationToken = default) where T : class, ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task<TResponse> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default) where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
    }

    public async ValueTask PublishMessageAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default)
        where
        TMessage : class, IMessage
    {
        await ExecuteScopeAsync(async sp =>
        {
            var bus = sp.GetRequiredService<IBus>();

            await bus.PublishAsync(message, headers, cancellationToken);
        });
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-masstransit/
    public async ValueTask WaitUntilConditionMet(Func<Task<bool>> conditionToMet, int? timeoutSecond = null)
    {
        var time = timeoutSecond ?? Timeout;

        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();
        while (!meet)
        {
            if (timeoutExpired)
            {
                throw new TimeoutException("Condition not met for the test.");
            }

            await Task.Delay(100);
            meet = await conditionToMet.Invoke();
            timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(time);
        }
    }

    public async Task<bool> IsPublished()
    {
        return await Harness.Published.Any<object>();
    }

    public async Task<bool> IsPublished<TMessage>()
        where TMessage : class, IMessage
    {
        return await Harness.Published.Any<TMessage>();
    }

    public async Task WaitForPublishing<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () => await IsPublished<TMessage>());
    }

    public async Task<bool> IsConsumed<TMessage>()
        where TMessage : class, IMessage
    {
        return await Harness.Consumed.Any<TMessage>();
    }

    public async Task<bool> IsConsumed<TMessage, TConsumedBy>()
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {
        var consumerHarness = ServiceProvider.GetRequiredService<IConsumerTestHarness<TConsumedBy>>();
        return await consumerHarness.Consumed.Any<TMessage>();
    }

    public async Task WaitForConsuming<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () => await IsConsumed<TMessage>());
    }

    public async Task WaitForConsuming<TMessage, TConsumedBy>()
        where TMessage : class
        where TConsumedBy : class, IConsumer
    {
        await WaitUntilConditionMet(async () => await IsConsumed<TMessage, TConsumedBy>());
    }

    // public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage>(
    //     Predicate<TMessage>? match = null)
    //     where TMessage : class, IMessage
    // {
    //     var hypothesis = Hypothesis
    //         .For<TMessage>()
    //         .Any(match ?? (_ => true));
    //
    //     ////https://stackoverflow.com/questions/55169197/how-to-use-masstransit-test-harness-to-test-consumer-with-constructor-dependency
    //     // Harness.Consumer(() => hypothesis.AsConsumer());
    //
    //     await Harness.SubscribeHandler<TMessage>(ctx =>
    //     {
    //         hypothesis.Test(ctx.Message).GetAwaiter().GetResult();
    //         return true;
    //     });
    //
    //     return hypothesis;
    // }
    //
    // public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage, TConsumer>(
    //     Predicate<TMessage>? match = null)
    //     where TMessage : class, IMessage
    //     where TConsumer : class, IConsumer<TMessage>
    // {
    //     var hypothesis = Hypothesis
    //         .For<TMessage>()
    //         .Any(match ?? (_ => true));
    //
    //     //https://stackoverflow.com/questions/55169197/how-to-use-masstransit-test-harness-to-test-consumer-with-constructor-dependency
    //     Harness.Consumer(() => hypothesis.AsConsumer<TMessage, TConsumer>(ServiceProvider));
    //
    //     return hypothesis;
    // }

    public async ValueTask ShouldProcessedOutboxPersistMessage<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                Guard.Against.Null(messagePersistenceService, nameof(messagePersistenceService));

                var filter = await messagePersistenceService.GetByFilterAsync(x =>
                    x.DeliveryType == MessageDeliveryType.Outbox &&
                    TypeMapper.GetTypeName(typeof(TMessage)) == x.DataType);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                if (res is true)
                {

                }
                return res;
            });
        });
    }

    public async ValueTask ShouldProcessedPersistInternalCommand<TInternalCommand>()
        where TInternalCommand : class, IInternalCommand
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                Guard.Against.Null(messagePersistenceService, nameof(messagePersistenceService));

                var filter = await messagePersistenceService.GetByFilterAsync(x =>
                    x.DeliveryType == MessageDeliveryType.Internal &&
                    TypeMapper.GetTypeName(typeof(TInternalCommand)) == x.DataType);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                return res;
            });
        });
    }

    public virtual async Task InitializeAsync()
    {
    }

    public virtual async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}

public class IntegrationTestFixture<TEntryPoint, TContext> : IntegrationTestFixture<TEntryPoint>
    where TContext : DbContext
    where TEntryPoint : class
{
    public async Task ExecuteTxContextAsync(Func<IServiceProvider, TContext, ValueTask> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteTxContextAsync<T>(Func<IServiceProvider, TContext, ValueTask<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public ValueTask ExecuteContextAsync(Func<TContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    public ValueTask ExecuteContextAsync(Func<TContext, ICommandProcessor, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    public ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ICommandProcessor, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public async ValueTask<int> InsertAsync<T>(params T[] entities) where T : class
    {
        return await ExecuteContextAsync(async db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3
        entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2,
        TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return await db.SaveChangesAsync();
        });
    }

    public ValueTask<T?> FindAsync<T>(object id) where T : class
    {
        return ExecuteContextAsync(db => db.Set<T>().FindAsync(id));
    }
}

public class IntegrationTestFixture<TEntryPoint, TWContext, TRContext> : IntegrationTestFixture<TEntryPoint, TWContext>
    where TWContext : DbContext
    where TRContext : MongoDbContext
    where TEntryPoint : class
{
    public ValueTask ExecuteReadContextAsync(Func<TRContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public ValueTask ExecuteReadContextAsync(Func<TRContext, ICommandProcessor, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ICommandProcessor, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));
}
