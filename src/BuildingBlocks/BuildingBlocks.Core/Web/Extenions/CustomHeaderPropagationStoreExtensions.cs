using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Web.HeaderPropagation;

namespace BuildingBlocks.Core.Web.Extenions;

public static class CustomHeaderPropagationStoreExtensions
{
    /// <summary>
    /// Get CorrelationId from header storage and throw if not exist.
    /// </summary>
    /// <param name="store"></param>
    /// <returns>Guid.</returns>
    public static Guid GetCorrelationId(this CustomHeaderPropagationStore store)
    {
        store.Headers.TryGetValue(MessageHeaders.CausationId, out var cid);

        if (string.IsNullOrEmpty(cid))
        {
            throw new System.Exception("CorrelationId is not found in the headers.");
        }

        return Guid.Parse(cid!);
    }
}
