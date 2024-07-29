using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Web.HeaderPropagation;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Core.Web.Extenions;

/// <summary>
/// Static helper class for <see cref="IConfiguration"/>.
/// </summary>
public static class CustomHeaderPropagationStoreExtensions
{
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
