using System.Data;
using System.Data.Common;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using Npgsql;

namespace Core.Persistence.Postgres;

public class EfNpgsqlConnectionFactory : IEfConnectionFactory
{
    private readonly string _connectionString;
    private DbConnection? _connection;

    public EfNpgsqlConnectionFactory(string connectionString)
    {
        Guard.Against.NullOrEmpty(connectionString);
        _connectionString = connectionString;
    }

    public async Task<DbConnection> GetOrCreateConnectionAsync()
    {
        if (_connection is null || _connection.State != ConnectionState.Open)
        {
            _connection = new NpgsqlConnection(_connectionString);
            await _connection.OpenAsync();
        }

        return _connection;
    }

    public void Dispose()
    {
        if (_connection is {State: ConnectionState.Open})
            _connection.Dispose();
    }
}
