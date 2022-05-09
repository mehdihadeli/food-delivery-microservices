using MediatR;

namespace BuildingBlocks.Abstractions.CQRS.Query;

public interface IQuery<out T> : IRequest<T>
    where T : notnull
{
}

// https://jimmybogard.com/mediatr-10-0-released/
public interface IStreamQuery<out T> : IStreamRequest<T>
    where T : notnull
{
}
