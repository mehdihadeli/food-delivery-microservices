using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductCategory.Events;

public record ChangingProductCategory(CategoryId CategoryId) : DomainEvent;

internal class ChangingProductCategoryValidationHandler :
    IDomainEventHandler<ChangingProductCategory>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ChangingProductCategoryValidationHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task Handle(ChangingProductCategory notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));
        Guard.Against.NegativeOrZero(notification.CategoryId, nameof(notification.CategoryId));
        Guard.Against.ExistsCategory(
            await _catalogDbContext.CategoryExistsAsync(notification.CategoryId, cancellationToken: cancellationToken),
            notification.CategoryId);
    }
}
