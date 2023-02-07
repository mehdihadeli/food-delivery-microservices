using Xunit.Sdk;

namespace Tests.Shared.XunitCategories;

// Ref: https://dateo-software.de/blog/test-categories-in-xunit
// https://www.brendanconnolly.net/organizing-tests-with-xunit-traits/
// https://github.com/xunit/samples.xunit/tree/main/TraitExtensibility

/// <summary>
/// Could filter by 'dotnet test --filter "Category=TestCategory"' in running tests in command line
/// </summary>
[TraitDiscoverer(CategoryTraitDiscoverer.DiscovererTypeName, XunitConstants.AssemblyName)]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class CategoryTraitAttribute : Attribute, ITraitAttribute
{
    public CategoryTraitAttribute(TestCategory category)
    {
        Name = category;
    }

    public TestCategory Name { get; }
}

public class CategoryTraitDiscoverer : ITraitDiscoverer
{
    private const string Key = "Category";

    public const string DiscovererTypeName =
        $"{XunitConstants.AssemblyName}.{nameof(XunitCategories)}.{nameof(CategoryTraitDiscoverer)}";

    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var categoryName = traitAttribute.GetNamedArgument<TestCategory?>("Name");

        if (categoryName is { })
            yield return new KeyValuePair<string, string>(Key, categoryName.ToString()!);
    }
}

public enum TestCategory
{
    Unit,
    Integration,
    EndToEnd,
    LoadTest,
    SkipCI
}
