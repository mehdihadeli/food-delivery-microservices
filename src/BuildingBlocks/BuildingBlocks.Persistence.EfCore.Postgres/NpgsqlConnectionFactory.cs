using System.Data;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using BuildingBlocks.Persistence.EfCore.Postgres;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Core.Persistence.Postgres;

public class NpgsqlConnectionFactory : IConnectionFactory
{
    private readonly PostgresOptions _options;
    private IDbConnection? _connection;

    public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
    {
        _options = Guard.Against.Null(options.Value, nameof(PostgresOptions));
        Guard.Against.NullOrEmpty(
            _options.ConnectionString,
            nameof(_options.ConnectionString),
            "ConnectionString can't be empty or null.");
    }

    public IDbConnection GetOrCreateConnection()
    {
        if (_connection is null || _connection.State != ConnectionState.Open)
        {
            _connection = new NpgsqlConnection(_options.ConnectionString);
            _connection.Open();
        }

        return _connection;
    }

    public void Dispose()
    {
        if (_connection is {State: ConnectionState.Open})
            _connection.Dispose();
    }
}
