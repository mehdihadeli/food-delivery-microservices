using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace FoodDelivery.Services.Identity.Shared.Extensions;

public static class UserManagerExtensions
{
    public static async Task<IPageList<TResult>> FindAllUsersByPageAsync<TResult>(
        this UserManager<ApplicationUser> userManager,
        IPageRequest request,
        ISieveProcessor sieveProcessor,
        Func<IQueryable<ApplicationUser>, IQueryable<TResult>> projectionFunc,
        CancellationToken cancellationToken
    )
        where TResult : class
    {
        // https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/
        // we don't use include for loading nested navigation because with mapping we load them explicitly
        return await userManager
            .Users.OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ApplyPagingAsync(
                request,
                sieveProcessor,
                projectionFunc,
                x => x.UserName,
                cancellationToken: cancellationToken
            );
    }
}
