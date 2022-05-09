namespace BuildingBlocks.Abstractions.Persistence.EfCore;

public interface IRetryDbContextExecution
{
    Task RetryOnExceptionAsync(Func<Task> operation);
    Task<TResult> RetryOnExceptionAsync<TResult>(Func<Task<TResult>> operation);
}
