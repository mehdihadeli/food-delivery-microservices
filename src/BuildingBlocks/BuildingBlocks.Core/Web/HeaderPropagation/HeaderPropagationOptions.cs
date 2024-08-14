namespace BuildingBlocks.Core.Web.HeaderPropagation;

public class CustomHeaderPropagationOptions
{
    public IList<string> HeaderNames { get; set; } = new List<string>();
}
