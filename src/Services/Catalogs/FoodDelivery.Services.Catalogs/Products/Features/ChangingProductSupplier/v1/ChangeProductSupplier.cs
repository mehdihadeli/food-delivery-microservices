using BuildingBlocks.Abstractions.Commands;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductSupplier.v1;

public record ChangeProductSupplier : ITxCommand<ChangeProductSupplierResult>;

public record ChangeProductSupplierResult;

public class ChangeProductSupplierCommandHandler : ICommandHandler<ChangeProductSupplier, ChangeProductSupplierResult>
{
    public ValueTask<ChangeProductSupplierResult> Handle(
        ChangeProductSupplier request,
        CancellationToken cancellationToken
    )
    {
        return ValueTask.FromResult<ChangeProductSupplierResult>(null!);
    }
}
