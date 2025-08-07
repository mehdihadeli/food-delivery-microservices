namespace Tests.Shared.XunitCategories;

/// <summary>
/// Filter by `dotnet test --filter "Bug=201"` or `dotnet test --filter "Category=Bug"`
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class BugTraitAttribute(int id) : Attribute
{
    public int Id { get; } = id;

    // For compatibility with previous filtering (Category=Bug)
    public string Category => "Bug";
}
