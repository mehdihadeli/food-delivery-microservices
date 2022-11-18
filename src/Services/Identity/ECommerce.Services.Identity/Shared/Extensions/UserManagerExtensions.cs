using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Identity.Shared.Extensions;

public static class UserManagerExtensions
{
    public static async Task<IReadOnlyList<ApplicationUser>> FindAllUserWithRoleAsync(
        this UserManager<ApplicationUser> userManager)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public static async Task<ListResultModel<TResult>> FindAllUsersByPageAsync<TResult>(
        this UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IPageRequest request,
        CancellationToken cancellationToken)
        where TResult : notnull
    {
        // https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/
        // we don't use include for loading nested navigation because with mapping we load them explicitly
        return await userManager.Users
            .OrderByDescending(x => x.CreatedAt)
            .ApplyIncludeList(request.Includes)
            .ApplyFilter(request.Filters)
            .AsNoTracking()
            .ApplyPagingAsync<ApplicationUser, TResult>(
                mapper.ConfigurationProvider,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);
    }

    public static async Task<ApplicationUser?> FindUserWithRoleByIdAsync(
        this UserManager<ApplicationUser> userManager,
        Guid userId)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(x => x.AccessTokens)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public static async Task<ApplicationUser?> FindUserWithRoleByUserNameAsync(
        this UserManager<ApplicationUser> userManager,
        string userName)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(x => x.AccessTokens)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }

    public static async Task<ApplicationUser?> FindUserWithRoleByEmailAsync(
        this UserManager<ApplicationUser> userManager,
        string email)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Email == email);
    }
}
