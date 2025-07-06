using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BuildingBlocks.Abstractions.Core.Diagnostics;

public interface IDiagnosticsProvider : IDisposable
{
    string InstrumentationName { get; }
    ActivitySource ActivitySource { get; }
    Meter Meter { get; }

    /// <summary>
    /// Allow adding a new custom listener.
    /// </summary>
    /// <param name="listener"></param>
    void AddCustomActivityListener(ActivityListener listener);

    /// <summary>
    ///  This method creates a minimal, no-frills ActivityListener that forces all activities to be Created and Sampled.
    ///  Ensures ActivitySource.CreateActivity() never returns null and can be used for unit tests.
    /// </summary>
    void AddEmptyListener();
}
