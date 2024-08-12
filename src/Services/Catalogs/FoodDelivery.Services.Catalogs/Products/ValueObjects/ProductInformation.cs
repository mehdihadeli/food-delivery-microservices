using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.ValueObjects;

public record ProductInformation
{
    // EF
    private ProductInformation() { }

    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;

    public static ProductInformation Of([NotNull] string? title, [NotNull] string? content)
    {
        // validations should be placed here instead of constructor
        title.NotBeNullOrWhiteSpace();
        content.NotBeNullOrWhiteSpace();

        return new ProductInformation { Title = title, Content = content };
    }

    public void Deconstruct(out string title, out string content) => (title, content) = (Title, Content);
}
