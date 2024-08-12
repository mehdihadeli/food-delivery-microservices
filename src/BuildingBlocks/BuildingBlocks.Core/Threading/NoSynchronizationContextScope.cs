namespace BuildingBlocks.Core.Threading;

public static class NoSynchronizationContextScope
{
    public static Disposable Enter()
    {
        var context = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        return new Disposable(context);
    }

    public readonly struct Disposable(SynchronizationContext? synchronizationContext) : IDisposable
    {
        public void Dispose() => SynchronizationContext.SetSynchronizationContext(synchronizationContext);
    }
}
