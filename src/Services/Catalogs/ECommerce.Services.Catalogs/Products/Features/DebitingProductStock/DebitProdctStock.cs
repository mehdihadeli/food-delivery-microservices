using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Shared.Contracts;

namespace ECommerce.Services.Catalogs.Products.Features.DebitingProductStock;

public record DebitProductStock(long ProductId, int Quantity) : ITxCommand;

internal class DebitProductStockValidator : AbstractValidator<DebitProductStock>
{
    public DebitProductStockValidator()
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

internal class DebitProductStockHandler : ICommandHandler<DebitProductStock>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public DebitProductStockHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = Guard.Against.Null(catalogDbContext, nameof(catalogDbContext));
    }

    public async Task<Unit> Handle(DebitProductStock command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var product =
            await _catalogDbContext.Products.SingleOrDefaultAsync(x => x.Id == command.ProductId, cancellationToken);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        Guard.Against.NotFound(product, new ProductCustomNotFoundException(command.ProductId));
        product!.DebitStock(command.Quantity);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
