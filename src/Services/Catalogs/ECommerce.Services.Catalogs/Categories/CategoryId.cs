using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Catalogs.Categories;

public record CategoryId : AggregateId
{
    // EF
    private CategoryId(long value) : base(value)
    {
    }

    public static implicit operator long(CategoryId id) => id.Value;

    // validations should be placed here instead of constructor
    public static CategoryId Of(long id) => new(Guard.Against.NegativeOrZero(id));
}
