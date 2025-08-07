using BuildingBlocks.Abstractions.Commands;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;
using FoodDelivery.Services.Shared;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using MassTransit;
using Saunter.Attributes;

namespace FoodDelivery.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;

[AsyncApi]
public class UserRegisteredConsumer(ICommandBus commandBus, IServiceProvider serviceProvider)
    : IConsumer<UserRegisteredV1>
{
    public async Task Consume(ConsumeContext<UserRegisteredV1> context)
    {
        var userRegistered = context.Message;
        if (userRegistered.Roles is null || !userRegistered.Roles.Contains(Authorization.Roles.User))
            return;

        await commandBus.SendAsync(new CreateCustomer(userRegistered.Email));
    }
}
