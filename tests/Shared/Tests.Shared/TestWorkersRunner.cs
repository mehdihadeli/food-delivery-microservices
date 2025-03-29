using Microsoft.Extensions.Hosting;

namespace Tests.Shared;

public class TestWorkersRunner(IEnumerable<IHostedService> workers)
{
    /// <summary>
    /// Resolves and starts all the workers manually.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task StartWorkersAsync(CancellationToken cancellationToken = default)
    {
        foreach (var worker in workers)
        {
            if (worker is BackgroundService backgroundWorker)
            {
                await worker.StartAsync(cancellationToken);
                if (backgroundWorker.ExecuteTask is not null)
                {
                    // here is actual waiting on task because this is our actual background task to wait
                    await backgroundWorker.ExecuteTask;
                }
            }
            else
            {
                await worker.StartAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Resolves and stops all the workers manually.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task StopWorkersAsync(CancellationToken cancellationToken = default)
    {
        foreach (var worker in workers)
        {
            await worker.StopAsync(cancellationToken);
        }
    }
}
