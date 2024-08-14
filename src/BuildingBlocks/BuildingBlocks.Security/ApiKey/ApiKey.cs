namespace BuildingBlocks.Security.ApiKey;

public class ApiKey(int id, string owner, string key, DateTime created, IReadOnlyCollection<string> roles)
{
    public int Id { get; } = id;
    public string Owner { get; } = owner ?? throw new ArgumentNullException(nameof(owner));
    public string Key { get; } = key ?? throw new ArgumentNullException(nameof(key));
    public DateTime Created { get; } = created;
    public IReadOnlyCollection<string> Roles { get; } = roles ?? throw new ArgumentNullException(nameof(roles));
}
