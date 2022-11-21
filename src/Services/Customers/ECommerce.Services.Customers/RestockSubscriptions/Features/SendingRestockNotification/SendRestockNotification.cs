using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.CQRS.Commands;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Email;
using BuildingBlocks.Email.Options;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Domain;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.SendingRestockNotification;

public record SendRestockNotification(long RestockSubscriptionId, int CurrentStock) : InternalCommand, ITxRequest;

internal class SendRestockNotificationValidator : AbstractValidator<SendRestockNotification>
{
    public SendRestockNotificationValidator()
    {
        RuleFor(x => x.RestockSubscriptionId)
            .NotEmpty();

        RuleFor(x => x.CurrentStock)
            .NotEmpty();
    }
}

internal class SendRestockNotificationHandler : ICommandHandler<SendRestockNotification>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailConfig;
    private readonly ILogger<SendRestockNotificationHandler> _logger;

    public SendRestockNotificationHandler(
        CustomersDbContext customersDbContext,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailConfig,
        ILogger<SendRestockNotificationHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _emailSender = emailSender;
        _emailConfig = emailConfig.Value;
        _logger = logger;
    }

    public async Task<Unit> Handle(SendRestockNotification command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, new RestockSubscriptionDomainException("Command cannot be null"));

        var restockSubscription =
            await _customersDbContext.RestockSubscriptions
                .FirstOrDefaultAsync(x => x.Id == command.RestockSubscriptionId, cancellationToken: cancellationToken);

        if (_emailConfig.Enable && restockSubscription is not null)
        {
            await _emailSender.SendAsync(
                new EmailObject(
                    restockSubscription.Email!,
                    _emailConfig.From,
                    "Restock Notification",
                    $"Your product {restockSubscription.ProductInformation.Name} is back in stock. Current stock is {command.CurrentStock}"));

            _logger.LogInformation("Restock notification sent to email {Email}", restockSubscription.Email);
        }

        return Unit.Value;
    }
}
