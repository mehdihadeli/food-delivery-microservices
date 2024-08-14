using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using Microsoft.AspNetCore.Identity;
using Sieve.Services;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUsers.v1;

internal record GetUsers : PageQuery<GetUsersResult>
{
    /// <summary>
    /// GetUsers with in-line validator.
    /// </summary>
    /// <param name="pageRequest"></param>
    /// <returns></returns>
    public static GetUsers Of(PageRequest pageRequest)
    {
        var (pageNumber, pageSize, filters, sortOrder) = pageRequest;

        return new GetUsersValidator().HandleValidation(
            new GetUsers
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder
            }
        );
    }
}

internal class GetUsersValidator : AbstractValidator<GetUsers>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

internal class GetUsersHandler(UserManager<ApplicationUser> userManager, IMapper mapper, ISieveProcessor sieveProcessor)
    : IQueryHandler<GetUsers, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        var users = await userManager.FindAllUsersByPageAsync<IdentityUserDto>(
            request,
            mapper,
            sieveProcessor,
            cancellationToken
        );

        return new GetUsersResult(users);
    }
}

internal record GetUsersResult(IPageList<IdentityUserDto> IdentityUsers);
