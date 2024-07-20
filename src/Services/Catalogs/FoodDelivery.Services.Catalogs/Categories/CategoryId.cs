using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Categories;

public record CategoryId : AggregateId
{
    // EF
    private CategoryId(long value)
        : base(value) { }

    public static implicit operator long(CategoryId id) => id.Value;

    // validations should be placed here instead of constructor
    public static CategoryId Of(long id) => new(id.NotBeNegativeOrZero());
}
