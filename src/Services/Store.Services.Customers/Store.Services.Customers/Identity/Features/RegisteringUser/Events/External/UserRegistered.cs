using BuildingBlocks.Abstractions.CQRS.Command;
using MassTransit;
using Store.Services.Customers.Customers.Features.CreatingCustomer;
using Store.Services.Shared.Identity.Users.Events.Integration;

namespace Store.Services.Customers.Identity.Features.RegisteringUser.Events.External;

internal class UserRegisteredConsumer : IConsumer<UserRegistered>
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
