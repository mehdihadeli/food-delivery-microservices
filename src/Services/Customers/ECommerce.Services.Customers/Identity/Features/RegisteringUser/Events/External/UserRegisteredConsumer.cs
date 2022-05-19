using BuildingBlocks.Abstractions.CQRS.Commands;
using MassTransit;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer;
using ECommerce.Services.Shared.Identity.Users.Events.Integration;

namespace ECommerce.Services.Customers.Identity.Features.RegisteringUser.Events.External;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly ICommandProcessor _commandProcessor;

    public UserRegisteredConsumer(ICommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var userRegistered = context.Message;
        if (userRegistered.Roles is null || !userRegistered.Roles.Contains(CustomersConstants.Role.User))
            return;

        await _commandProcessor.SendAsync(new CreateCustomer(userRegistered.Email));
    }
}
