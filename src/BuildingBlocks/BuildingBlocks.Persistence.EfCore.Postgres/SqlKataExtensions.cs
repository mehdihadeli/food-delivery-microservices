using System.Collections.Immutable;
using System.Data;
using Dapper;
using SqlKata;
using SqlKata.Compilers;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

// Ref: https://github.com/sqlkata/querybuilder
// Ref: https://github.com/fulviocanducci/Canducci.SqlKata.Dapper
public static class SqlKataExtensions
{
    private static readonly PostgresCompiler _postgresCompiler = new PostgresCompiler();

    public static async Task<IReadOnlyList<T>> QueryAsync<T>(this IDbConnection connection, Action<Query> source)
    {
        var query = new Query();
        source.Invoke(query);

        SqlResult compileResult = _postgresCompiler.Compile(query);

        var result = await connection.QueryAsync<T>(compileResult.Sql, compileResult.NamedBindings);

        return result.ToImmutableList();
    }

    public static Task<T> QueryOneAsync<T>(this IDbConnection connection)
    {
        SqlResult compile = _postgresCompiler.Compile(new Query());
        return connection.QueryFirstOrDefaultAsync<T>(compile.Sql, compile.NamedBindings);
    }
}
