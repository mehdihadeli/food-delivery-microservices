using Mediator;

namespace BuildingBlocks.Core.Messages;

public class NullMediator : IMediator
{
    public ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = new()
    )
    {
        throw new NotImplementedException();
    }

    public ValueTask<TResponse> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = new()
    )
    {
        throw new NotImplementedException();
    }

    public ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public ValueTask<object?> Send(object message, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamQuery<TResponse> query,
        CancellationToken cancellationToken = new()
    )
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = new()
    )
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamCommand<TResponse> command,
        CancellationToken cancellationToken = new()
    )
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new())
        where TNotification : INotification
    {
        throw new NotImplementedException();
    }

    public ValueTask Publish(object notification, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }
}
