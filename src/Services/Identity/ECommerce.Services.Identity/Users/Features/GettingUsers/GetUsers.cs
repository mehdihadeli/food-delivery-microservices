using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Types;
using ECommerce.Services.Identity.Shared.Models;
using ECommerce.Services.Identity.Users.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Identity.Users.Features.GettingUsers;

public record GetUsers : ListQuery<GetUsersResult>;


public class GetUsersValidator : AbstractValidator<GetUsers>
{
    public GetUsersValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUsersHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<GetUsersResult> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        var customer = await _userManager.Users
            .OrderByDescending(x => x.CreatedAt)
            .ApplyIncludeList(request.Includes)
            .ApplyFilter(request.Filters)
            .AsNoTracking()
            .ApplyPagingAsync<ApplicationUser, IdentityUserDto>(
                _mapper.ConfigurationProvider,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

        return new GetUsersResult(customer);
    }
}

public record GetUsersResult(ListResultModel<IdentityUserDto> IdentityUsers);
