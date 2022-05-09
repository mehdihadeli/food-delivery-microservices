namespace BuildingBlocks.Security.Jwt;

public class RolePolicy
{
    public RolePolicy(string name, IReadOnlyList<string>? roles)
    {
        Name = name;
        Roles = roles ?? new List<string>();
    }

    public string Name { get; set; }
    public IReadOnlyList<string> Roles { get; set; }
}