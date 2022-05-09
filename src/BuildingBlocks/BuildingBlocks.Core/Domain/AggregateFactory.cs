using System.Linq.Expressions;

namespace BuildingBlocks.Core.Domain;

public static class AggregateFactory<T>
{
    private static readonly Func<T> _constructor = CreateTypeConstructor();

    private static Func<T> CreateTypeConstructor()
    {
        try
        {
            var newExpr = Expression.New(typeof(T));
            var func = Expression.Lambda<Func<T>>(newExpr);
            return func.Compile();
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static T CreateAggregate()
    {
        if (_constructor == null)
            throw new System.Exception($"Aggregate {typeof(T).Name} does not have a parameterless constructor");
        return _constructor();
    }
}
