using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Email;
using BuildingBlocks.Email.Options;
using FluentValidation;
using FoodDelivery.Services.Customers.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.SendingRestockNotification.v1;

public record SendRestockNotification(long RestockSubscriptionId, int CurrentStock) : InternalCommand, ITxRequest;

public class SendRestockNotificationValidator : AbstractValidator<SendRestockNotification>
{
    public SendRestockNotificationValidator()
    {
        RuleFor(x => x.RestockSubscriptionId).NotEmpty();

        RuleFor(x => x.CurrentStock).NotEmpty();
    }
}

public class SendRestockNotificationHandler(
    CustomersDbContext customersDbContext,
    IEmailSender emailSender,
    IOptions<EmailOptions> emailConfig,
    ILogger<SendRestockNotificationHandler> logger
) : BuildingBlocks.Abstractions.Commands.ICommandHandler<SendRestockNotification>
{
    private readonly EmailOptions _emailConfig = emailConfig.Value;

    public async ValueTask<Unit> Handle(SendRestockNotification command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var restockSubscription = await customersDbContext.RestockSubscriptions.FirstOrDefaultAsync(
            x => x.Id == command.RestockSubscriptionId,
            cancellationToken: cancellationToken
        );

        if (_emailConfig.Enable && restockSubscription is not null)
        {
            await emailSender.SendAsync(
                new EmailObject(
                    restockSubscription.Email!,
                    _emailConfig.From,
                    "Restock Notification",
                    $"Your product {restockSubscription.ProductInformation.Name} is back in stock. Current stock is {command.CurrentStock}"
                )
            );

            logger.LogInformation("Restock notification sent to email {Email}", restockSubscription.Email);
        }

        return Unit.Value;
    }
}
