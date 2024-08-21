using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.SendingRestockNotification.v1;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;

internal record ProcessRestockNotification(long ProductId, int CurrentStock) : ITxCommand
{
    public static ProcessRestockNotification Of(long productId, int currentStock)
    {
        productId.NotBeNegativeOrZero();
        currentStock.NotBeNegativeOrZero();

        return new ProcessRestockNotification(productId, currentStock);
    }
}

internal class ProcessRestockNotificationValidator : AbstractValidator<ProcessRestockNotification>
{
    public ProcessRestockNotificationValidator()
    {
        RuleFor(x => x.CurrentStock).NotEmpty();

        RuleFor(x => x.ProductId).NotEmpty();
    }
}

internal class ProcessRestockNotificationHandler(
    CustomersDbContext customersDbContext,
    ICommandBus commandBus,
    ILogger<ProcessRestockNotificationHandler> logger
) : ICommandHandler<ProcessRestockNotification>
{
    public async Task Handle(ProcessRestockNotification command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var subscribedCustomers = customersDbContext.RestockSubscriptions.Where(x =>
            x.ProductInformation.Id == command.ProductId && !x.Processed
        );

        if (!await subscribedCustomers.AnyAsync(cancellationToken: cancellationToken))
            return;

        foreach (var restockSubscription in subscribedCustomers)
        {
            restockSubscription!.MarkAsProcessed(DateTime.Now);

            // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
            // schedule `SendRestockNotification` for running as a internal command after commenting transaction
            await commandBus.ScheduleAsync(
                new SendRestockNotification(restockSubscription.Id, command.CurrentStock),
                cancellationToken
            );
        }

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Marked restock subscriptions as processed");
    }
}
