using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record DebitProductStock(long ProductId, int Quantity) : ITxCommand, ICommand
{
    public static DebitProductStock Of(long productId, int quantity)
    {
        return new DebitProductStockValidator().HandleValidation(new DebitProductStock(productId, quantity));
    }
}

public class DebitProductStockValidator : AbstractValidator<DebitProductStock>
{
    public DebitProductStockValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId must be greater than 0");
    }
}

public class DebitProductStockHandler(ICatalogDbContext catalogDbContext)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<DebitProductStock>
{
    public async ValueTask<Unit> Handle(DebitProductStock request, CancellationToken cancellationToken)
    {
        request.NotBeNull();

        var (productId, quantity) = request;

        var product = await catalogDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(productId);

        product.DebitStock(quantity);

        await catalogDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
