using MediatR;

namespace BuildingBlocks.Abstractions.CQRS.Commands;

public interface ICommand : IRequest { }

public interface ICommand<out T> : IRequest<T>
    where T : notnull { }
