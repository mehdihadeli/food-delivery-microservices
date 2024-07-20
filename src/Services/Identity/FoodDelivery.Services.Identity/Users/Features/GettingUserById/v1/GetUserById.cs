using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUserById.v1;

internal record GetUserById(Guid Id) : IQuery<GetUserByIdResult>
{
    public static GetUserById Of(Guid id) => new GetUserById(id.NotBeEmpty());
}

internal class GetUserByIdValidator : AbstractValidator<GetUserById>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("InternalCommandId is required.");
    }
}

internal class GetUserByIdHandler : IQueryHandler<GetUserById, GetUserByIdResult>
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<GetUserByIdResult> Handle(GetUserById query, CancellationToken cancellationToken)
    {
        query.NotBeNull();

        var identityUser = await _userManager.FindUserWithRoleByIdAsync(query.Id);
        identityUser.NotBeNull(new IdentityUserNotFoundException(query.Id));

        var identityUserDto = _mapper.Map<IdentityUserDto>(identityUser);

        return new GetUserByIdResult(identityUserDto);
    }
}

internal record GetUserByIdResult(IdentityUserDto IdentityUser);
