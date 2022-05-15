namespace BuildingBlocks.Core.Web;

public class AppOptions
{
    public string? Name { get; set; }
    public string? ApiAddress { get; set; }
    public string? Instance { get; set; }
    public string? Version { get; set; }
    public bool DisplayBanner { get; set; } = true;
    public bool DisplayVersion { get; set; } = true;
    public string? Description { get; set; }
}
