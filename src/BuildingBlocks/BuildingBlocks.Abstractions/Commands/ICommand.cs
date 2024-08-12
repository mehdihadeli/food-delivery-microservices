using MediatR;

namespace BuildingBlocks.Abstractions.Commands;

public interface ICommand : IRequest;

public interface ICommand<out T> : IRequest<T>
    where T : notnull;
