using System.Diagnostics;
using OpenTelemetry;

namespace BuildingBlocks.OpenTelemetry;

// https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/1730
public sealed class FixHttpRouteProcessor : BaseProcessor<Activity>
{
    private const string HttpRequestMethodTag = "http.request.method";
    private const string UrlPathTag = "url.path";
    private const string HttpRouteTag = "http.route";
    private const string NameTag = "name";
    private const string RequestNameTag = "request.name";

    public override void OnEnd(Activity activity)
    {
        if (activity.Kind != ActivityKind.Server)
        {
            return;
        }

        var method = activity.GetTagItem(HttpRequestMethodTag)?.ToString();
        var path = activity.GetTagItem(UrlPathTag)?.ToString();

        if (method is null || path is null)
        {
            return;
        }

        var displayName = $"{method} {path}";
        activity.DisplayName = displayName;
        activity.SetTag(HttpRouteTag, path);
        activity.SetTag(NameTag, displayName);
        activity.SetTag(RequestNameTag, displayName);
    }
}
