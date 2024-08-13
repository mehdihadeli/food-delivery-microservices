using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Products.Features.DebitingProductStock.V1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
internal record DebitProductStock(long ProductId, int Quantity) : ITxCommand
{
    public static DebitProductStock Of(long productId, int quantity)
    {
        return new DebitProductStockValidator().HandleValidation(new DebitProductStock(productId, quantity));
    }
}

internal class DebitProductStockValidator : AbstractValidator<DebitProductStock>
{
    public DebitProductStockValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId must be greater than 0");
    }
}

internal class DebitProductStockHandler(ICatalogDbContext catalogDbContext) : ICommandHandler<DebitProductStock>
{
    public async Task Handle(DebitProductStock command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (productId, quantity) = command;

        var product = await catalogDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(productId);

        product.DebitStock(quantity);

        await catalogDbContext.SaveChangesAsync(cancellationToken);
    }
}
