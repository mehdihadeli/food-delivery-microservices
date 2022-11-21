using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Customers.Customers.Models;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;

public record CustomerCreated(Customer Customer) : DomainEvent;

internal class CustomerCreatedHandler : IDomainEventHandler<CustomerCreated>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IMapper _mapper;

    public CustomerCreatedHandler(ICommandProcessor commandProcessor, IMapper mapper)
    {
        _commandProcessor = commandProcessor;
        _mapper = mapper;
    }

    public Task Handle(CustomerCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var mongoReadCommand = _mapper.Map<CreateMongoCustomerReadModels>(notification.Customer);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        return _commandProcessor.ScheduleAsync(new IInternalCommand[] { mongoReadCommand }, cancellationToken);
    }
}
