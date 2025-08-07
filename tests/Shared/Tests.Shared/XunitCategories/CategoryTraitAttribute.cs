namespace Tests.Shared.XunitCategories;

/// <summary>
/// Enables filtering via 'dotnet test --filter "Category=Unit"' etc.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class CategoryTraitAttribute : Attribute
{
    public CategoryTraitAttribute(TestCategory name) => Name = name;

    // Exposed as trait "Category"
    public TestCategory Name { get; }

    // For traditional trait-key for test filters: "Category"
    public string Category => Name.ToString();
}

public enum TestCategory
{
    Unit,
    Integration,
    EndToEnd,
    LoadTest,
    SkipCI,
}
