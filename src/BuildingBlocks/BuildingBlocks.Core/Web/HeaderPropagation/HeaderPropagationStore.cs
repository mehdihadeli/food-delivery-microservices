using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Core.Web.HeaderPropagation;

// Ref: https://gist.github.com/davidfowl/c34633f1ddc519f030a1c0c5abe8e867
// https://vainolo.com/2022/02/23/storing-context-data-in-c-using-asynclocal/

// Values on an instance of HeaderPropagationStore which will be unique per HTTP async request and other requests processed concurrently will have their own separate HeaderPropagationStore instances. This means the data is not shared between requests, preserving isolation and thread safety.
public class HeaderPropagationStore
{
    private static readonly AsyncLocal<IDictionary<string, StringValues>?> _headers = new();

    public IDictionary<string, StringValues> Headers
    {
        get { return _headers.Value ??= new Dictionary<string, StringValues>(); }
        set { _headers.Value = value; }
    }
}
