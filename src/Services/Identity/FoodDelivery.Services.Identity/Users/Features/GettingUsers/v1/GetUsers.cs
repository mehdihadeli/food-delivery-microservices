using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Validation.Extensions;
using FoodDelivery.Services.Identity.Shared.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FluentValidation;
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

internal class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;

    public GetUsersHandler(UserManager<ApplicationUser> userManager, IMapper mapper, ISieveProcessor sieveProcessor)
    {
        _userManager = userManager;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }

    public async Task<GetUsersResult> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        var users = await _userManager.FindAllUsersByPageAsync<IdentityUserDto>(
            request,
            _mapper,
            _sieveProcessor,
            cancellationToken
        );

        return new GetUsersResult(users);
    }
}

internal record GetUsersResult(IPageList<IdentityUserDto> IdentityUsers);
