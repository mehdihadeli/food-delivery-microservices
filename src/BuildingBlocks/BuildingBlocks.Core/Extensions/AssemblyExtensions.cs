using System.Reflection;

namespace BuildingBlocks.Core.Extensions;

public static class AssemblyExtensions
{
    /// <summary>
    /// Finds all assemblies that reference the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to check for references.</param>
    /// <returns>An array of assemblies that reference the specified assembly.</returns>
    public static Assembly[] GetReferencingAssemblies(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var referencingAssemblies = assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => Assembly.Load(assemblyName))
            .ToList();
        referencingAssemblies.Add(assembly);

        return referencingAssemblies.ToArray();
    }

    // ref: https://stackoverflow.com/a/7889272/581476
    // https://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
    public static IEnumerable<Type?> GetLoadableTypes(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }
}
