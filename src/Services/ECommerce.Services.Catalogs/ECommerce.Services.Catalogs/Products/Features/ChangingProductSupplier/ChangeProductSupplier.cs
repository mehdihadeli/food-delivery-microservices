using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductSupplier;

internal record ChangeProductSupplier : ITxCommand<ChangeProductSupplierResult>;

internal record ChangeProductSupplierResult;

internal class ChangeProductSupplierCommandHandler :
    ICommandHandler<ChangeProductSupplier, ChangeProductSupplierResult>
{
    public Task<ChangeProductSupplierResult> Handle(
        ChangeProductSupplier request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductSupplierResult>(null!);
    }
}
