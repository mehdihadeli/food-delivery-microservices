using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;

public record ProductInformation
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private ProductInformation() { }

    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;

    public static ProductInformation Of(string title, string content)
    {
        // validations should be placed here instead of constructor
        title.NotBeNullOrWhiteSpace();
        content.NotBeNullOrWhiteSpace();

        return new ProductInformation { Title = title, Content = content };
    }

    public void Deconstruct(out string title, out string content) => (title, content) = (Title, Content);
}
