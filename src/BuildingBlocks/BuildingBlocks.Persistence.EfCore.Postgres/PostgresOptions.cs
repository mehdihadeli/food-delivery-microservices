using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public class PostgresOptions
{
    public string ConnectionString { get; set; } = default!;
    public bool UseInMemory { get; set; }
    public string? MigrationAssembly { get; set; } = null!;
}
