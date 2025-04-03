using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Data;
using Mediator;
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

public class DeleteRestockSubscriptionValidator : AbstractValidator<DeleteRestockSubscription>
{
    public DeleteRestockSubscriptionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteRestockSubscriptionHandler(
    CustomersDbContext customersDbContext,
    ILogger<DeleteRestockSubscriptionHandler> logger
) : BuildingBlocks.Abstractions.Commands.ICommandHandler<DeleteRestockSubscription>
{
    public async ValueTask<Unit> Handle(DeleteRestockSubscription command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var exists = await customersDbContext
            .RestockSubscriptions.IgnoreAutoIncludes()
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (exists is null)
        {
            throw new RestockSubscriptionNotFoundException(command.Id);
        }

        customersDbContext.Entry(exists).State = EntityState.Deleted;
        customersDbContext.Entry(exists.ProductInformation).State = EntityState.Unchanged;

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("RestockSubscriptionReadModel with id '{InternalCommandId} removed.'", command.Id);

        return Unit.Value;
    }
}
