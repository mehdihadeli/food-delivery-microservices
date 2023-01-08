using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using FluentValidation;
using MediatR;

namespace ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.v1;

public record ReplenishProductStock(long ProductId, int Quantity) : ITxCommand;

internal class ReplenishingProductStockValidator : AbstractValidator<ReplenishProductStock>
{
    public ReplenishingProductStockValidator()
    {
        // https://docs.fluentvalidation.net/en/latest/conditions.html#stop-vs-stoponfirstfailure
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId must be greater than 0");
    }
}

internal class ReplenishingProductStockHandler : ICommandHandler<ReplenishProductStock>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ReplenishingProductStockHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = Guard.Against.Null(catalogDbContext, nameof(catalogDbContext));
    }

    public async Task<Unit> Handle(ReplenishProductStock command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var product = await _catalogDbContext.FindProductByIdAsync(ProductId.Of(command.ProductId));
        Guard.Against.NotFound(product, new ProductNotFoundException(command.ProductId));

        product!.ReplenishStock(command.Quantity);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
