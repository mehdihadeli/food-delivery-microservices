namespace Tests.Shared.XunitCategories;

/// <summary>
/// Enables filtering via 'dotnet test --filter "Category=Feature"' and 'dotnet test --filter "Feature=201"'
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class FeatureTraitAttribute : Attribute
{
    public FeatureTraitAttribute(int id) => Id = id;

    // xUnit 3 exposes all public get-only properties as traits
    public int Id { get; }

    // For traditional trait-key for test filters: "Feature"
    public int Feature => Id;

    // For compatibility with legacy "Category" groupings
    public string Category => "Feature";
}
