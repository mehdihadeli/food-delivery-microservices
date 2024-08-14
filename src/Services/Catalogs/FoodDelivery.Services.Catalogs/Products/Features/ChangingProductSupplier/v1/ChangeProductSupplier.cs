using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductSupplier.v1;

internal record ChangeProductSupplier : ITxCommand<ChangeProductSupplierResult>;

internal record ChangeProductSupplierResult;

internal class ChangeProductSupplierCommandHandler : ICommandHandler<ChangeProductSupplier, ChangeProductSupplierResult>
{
    public Task<ChangeProductSupplierResult> Handle(ChangeProductSupplier request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductSupplierResult>(null!);
    }
}
