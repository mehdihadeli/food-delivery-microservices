using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Web.HeaderPropagation;

public class CustomHeaderPropagationStore
{
    private static readonly AsyncLocal<IDictionary<string, StringValues>?> _headers =
        new AsyncLocal<IDictionary<string, StringValues>?>();

    public IDictionary<string, StringValues> Headers
    {
        get
        {
            if (_headers.Value is null)
            {
                _headers.Value = new Dictionary<string, StringValues>();
            }

            return _headers.Value;
        }
        set { _headers.Value = value; }
    }
}
