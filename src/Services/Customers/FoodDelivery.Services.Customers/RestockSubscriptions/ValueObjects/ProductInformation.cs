using System.Diagnostics.CodeAnalysis;
using FoodDelivery.Services.Customers.Products;
using Squirrel.Core.Extensions;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;

// Here versioning Name is not important for us so we can save it on DB
public record ProductInformation
{
    // EF
    public ProductInformation() { }

    public string Name { get; private set; } = default!;
    public ProductId Id { get; private set; } = default!;

    public static ProductInformation Of([NotNull] ProductId? id, [NotNull] string? name)
    {
        name.NotBeNullOrWhiteSpace();
        id.NotBeNull();

        return new ProductInformation { Name = name, Id = id };
    }
}
