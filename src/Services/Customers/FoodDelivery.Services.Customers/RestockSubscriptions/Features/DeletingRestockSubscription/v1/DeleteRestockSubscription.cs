using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Data;
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

internal class DeleteRestockSubscriptionHandler(
    CustomersDbContext customersDbContext,
    ILogger<DeleteRestockSubscriptionHandler> logger
) : ICommandHandler<DeleteRestockSubscription>
{
    public async Task Handle(DeleteRestockSubscription command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var exists = await customersDbContext
            .RestockSubscriptions.IgnoreAutoIncludes()
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (exists is null)
        {
            throw new RestockSubscriptionNotFoundException(command.Id);
        }

        // for raising a deleted domain event
        exists.Delete();

        customersDbContext.Entry(exists).State = EntityState.Deleted;
        customersDbContext.Entry(exists.ProductInformation).State = EntityState.Unchanged;

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("RestockSubscription with id '{InternalCommandId} removed.'", command.Id);
    }
}
