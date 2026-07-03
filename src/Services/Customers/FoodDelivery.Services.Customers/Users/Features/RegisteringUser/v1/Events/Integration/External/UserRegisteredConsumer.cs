using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Integration.Wolverine;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;
using FoodDelivery.Services.Shared;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Microsoft.AspNetCore.HeaderPropagation;
using Saunter.Attributes;
using Wolverine;
using ICommandBus = BuildingBlocks.Abstractions.Commands.ICommandBus;

namespace FoodDelivery.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;

[AsyncApi]
public class UserRegisteredConsumer(ICommandBus commandBus, HeaderPropagationValues headerPropagationValues)
{
    public async Task Handle(UserRegisteredV1 message, Envelope envelope)
    {
        await WolverineConsumerExecutor.ExecuteAsync(
            message,
            envelope,
            headerPropagationValues,
            async userRegistered =>
            {
                if (userRegistered.Roles is null || !userRegistered.Roles.Contains(Authorization.Roles.User))
                {
                    return;
                }

                await commandBus.SendAsync(new CreateCustomer(userRegistered.Email));
            }
        );
    }
}
