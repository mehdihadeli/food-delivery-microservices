using Scrutor;

namespace BuildingBlocks.Core.Extensions;

public static class ServiceTypeSelectorExtensions
{
    public static ILifetimeSelector AsClosedTypeOf(this IServiceTypeSelector selector, Type closedType)
    {
        // As(Func<Type, IEnumerable<Type>> selector)
        return _ = selector.As(t =>
            {
                var types = t.GetInterfaces()
                    .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == closedType)
                    .Select(implementedInterface =>
                        implementedInterface.GenericTypeArguments.Any(a => a.IsTypeDefinition)
                            ? implementedInterface
                            : implementedInterface.GetGenericTypeDefinition())
                    .Distinct();
                var result = types.ToList();

                return result;
            }
        );
    }
}
