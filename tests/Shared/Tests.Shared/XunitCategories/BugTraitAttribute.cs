using Xunit.Sdk;

namespace Tests.Shared.XunitCategories;


/// <summary>
/// Could filter by `dotnet test --filter "Category=Bug"` and `dotnet test --filter "Bug=201"` in running tests in command line
/// </summary>
[TraitDiscoverer(BugTraitDiscoverer.DiscovererTypeName, XunitConstants.AssemblyName)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class BugTraitAttribute : Attribute, ITraitAttribute
{
    public BugTraitAttribute(int id)
    {
        Id = id;
    }

    public int Id { get; }
}

internal class BugTraitDiscoverer : ITraitDiscoverer
{
    private const string Key = "Bug";
    internal const string DiscovererTypeName =
        $"{XunitConstants.AssemblyName}.{nameof(XunitCategories)}.{nameof(BugTraitDiscoverer)}";

    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var id = traitAttribute.GetNamedArgument<int?>("Id");

        yield return new KeyValuePair<string, string>("Category", Key);

        if (id is { })
            yield return new KeyValuePair<string, string>(Key, id.ToString()!);
    }
}
