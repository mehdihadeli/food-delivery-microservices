using System.Data.Common;

namespace BuildingBlocks.Abstractions.Persistence.EfCore;

public interface IEfConnectionFactory : IDisposable
{
    Task<DbConnection> GetOrCreateConnectionAsync();
}
