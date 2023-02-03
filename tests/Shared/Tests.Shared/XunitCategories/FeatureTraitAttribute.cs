using Xunit.Sdk;

namespace Tests.Shared.XunitCategories;

/// <summary>
/// Could filter by `dotnet test --filter "Category=Feature"` and `dotnet test --filter "Feature=201"` in running tests in command line
/// </summary>
[TraitDiscoverer(FeatureTraitDiscoverer.DiscovererTypeName, XunitConstants.AssemblyName)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class FeatureTraitAttribute : Attribute, ITraitAttribute
{
    public FeatureTraitAttribute(int id)
    {
        Id = id;
    }

    public int Id { get; }
}

internal class FeatureTraitDiscoverer : ITraitDiscoverer
{
    private const string Key = "Feature";
    internal const string DiscovererTypeName =
        $"{XunitConstants.AssemblyName}.{nameof(XunitCategories)}.{nameof(FeatureTraitDiscoverer)}";

    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var id = traitAttribute.GetNamedArgument<int?>("Id");

        yield return new KeyValuePair<string, string>("Category", Key);

        if (id is { })
            yield return new KeyValuePair<string, string>(Key, id.ToString()!);
    }
}
