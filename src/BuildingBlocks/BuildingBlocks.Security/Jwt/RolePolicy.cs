namespace BuildingBlocks.Security.Jwt;

public class RolePolicy(string name, IReadOnlyList<string>? roles)
{
    public string Name { get; set; } = name;
    public IReadOnlyList<string> Roles { get; set; } = roles ?? new List<string>();
}
