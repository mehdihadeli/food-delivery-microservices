using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry.Context.Propagation;

namespace Shared.Observability;

public class PropagationContextJsonConverter : JsonConverter<PropagationContext>
{
    private const string TraceParentPropertyName = "traceparent";
    private const string TraceStatePropertyName = "tracestate";

    public override PropagationContext Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a JSON object.");
        }

        string? traceParent = null;
        string? traceState = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();

                reader.Read(); // Move to the value
                if (propertyName == TraceParentPropertyName)
                {
                    traceParent = reader.GetString();
                }
                else if (propertyName == TraceStatePropertyName)
                {
                    traceState = reader.GetString();
                }
            }
        }

        var headers = new Dictionary<string, string?>
        {
            { TraceParentPropertyName, traceParent },
            { TraceStatePropertyName, traceState },
        };

        return TelemetryPropagator.Extract(headers, ExtractTraceContextFromHeaders);
    }

    public override void Write(Utf8JsonWriter writer, PropagationContext value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        value.Inject(
            new Dictionary<string, string>(),
            (carrier, key, val) =>
            {
                writer.WriteString(key, val);
            }
        );

        writer.WriteEndObject();
    }

    private static IEnumerable<string> ExtractTraceContextFromHeaders(Dictionary<string, string?> headers, string key)
    {
        return headers.TryGetValue(key, out var value) && value != null ? new[] { value } : Array.Empty<string>();
    }
}
