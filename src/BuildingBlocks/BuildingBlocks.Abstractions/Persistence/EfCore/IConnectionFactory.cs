using System.Data.Common;

namespace BuildingBlocks.Abstractions.Persistence.EfCore;

public interface IConnectionFactory : IDisposable
{
    Task<DbConnection> GetOrCreateConnectionAsync();
}
