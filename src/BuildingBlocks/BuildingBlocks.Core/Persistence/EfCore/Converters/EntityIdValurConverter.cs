using System.Reflection;
using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Core.Persistence.EfCore.Converters;

// https://stackoverflow.com/questions/708952/how-to-instantiate-an-object-with-a-private-constructor-in-c
public class EntityIdValurConverter<TEntityId, TId>(ConverterMappingHints mappingHints = null!)
    : ValueConverter<TEntityId, TId>(id => id.Value, value => Create(value), mappingHints)
    where TEntityId : EntityId<TId>
{
    // instantiate EntityId and pass id to its protected or private constructor
    private static TEntityId Create(TId id) =>
        (
            Activator.CreateInstance(
                typeof(TEntityId),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object?[] { id },
                null,
                null
            ) as TEntityId
        )!;
}
