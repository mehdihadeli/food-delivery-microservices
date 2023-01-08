using System.Reflection;

namespace BuildingBlocks.Core.Extensions;

public static class AssemblyExtensions
{
    // ref: https://stackoverflow.com/a/7889272/581476
    // https://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
    public static IEnumerable<Type?> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));
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
