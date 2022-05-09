using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BuildingBlocks.Abstractions.Persistence;

public interface IDbFacadeResolver
{
    DatabaseFacade Database { get; }
}
