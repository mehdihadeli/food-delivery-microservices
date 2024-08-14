using System.Reflection;
using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Core.Persistence.EfCore.Converters;

// https://stackoverflow.com/questions/708952/how-to-instantiate-an-object-with-a-private-constructor-in-c
public class AggregateIdValueConverter<TAggregateId, TId>(ConverterMappingHints mappingHints = null!)
    : ValueConverter<TAggregateId, TId>(id => id.Value, value => Create(value), mappingHints)
    where TAggregateId : AggregateId<TId>
{
    // instantiate AggregateId and pass id to its protected or private constructor
    private static TAggregateId Create(TId id) =>
        (
            Activator.CreateInstance(
                typeof(TAggregateId),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object?[] { id },
                null,
                null
            ) as TAggregateId
        )!;
}
