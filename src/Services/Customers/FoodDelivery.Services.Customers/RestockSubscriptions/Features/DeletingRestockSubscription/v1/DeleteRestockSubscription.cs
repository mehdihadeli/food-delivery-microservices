using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

public record DeleteRestockSubscription(long Id) : ITxCommand
{
    public static DeleteRestockSubscription Of(long id)
    {
        id.NotBeNegativeOrZero();
        return new DeleteRestockSubscription(id);
    }
}

internal class DeleteRestockSubscriptionValidator : AbstractValidator<DeleteRestockSubscription>
{
    public DeleteRestockSubscriptionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

internal class DeleteRestockSubscriptionHandler : ICommandHandler<DeleteRestockSubscription>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<DeleteRestockSubscriptionHandler> _logger;

    public DeleteRestockSubscriptionHandler(
        CustomersDbContext customersDbContext,
        ILogger<DeleteRestockSubscriptionHandler> logger
    )
    {
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteRestockSubscription command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var exists = await _customersDbContext.RestockSubscriptions
            .IgnoreAutoIncludes()
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (exists is null)
        {
            throw new RestockSubscriptionNotFoundException(command.Id);
        }

        // for raising a deleted domain event
        exists!.Delete();

        _customersDbContext.Entry(exists).State = EntityState.Deleted;
        _customersDbContext.Entry(exists.ProductInformation).State = EntityState.Unchanged;

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("RestockSubscription with id '{InternalCommandId} removed.'", command.Id);

        return Unit.Value;
    }
}
