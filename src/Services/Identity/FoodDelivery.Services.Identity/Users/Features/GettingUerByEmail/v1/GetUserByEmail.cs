using AutoMapper;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUerByEmail.V1;

internal record GetUserByEmail(string Email) : IQuery<GetUserByEmailResult>
{
    /// <summary>
    /// GetUserByEmail with in-line validation.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static GetUserByEmail Of(string? email)
    {
        return new GetUserByIdValidator().HandleValidation(new GetUserByEmail(email!));
    }
}

internal class GetUserByIdValidator : AbstractValidator<GetUserByEmail>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email address is not valid");
    }
}

internal class GetUserByEmailHandler : IQueryHandler<GetUserByEmail, GetUserByEmailResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUserByEmailHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<GetUserByEmailResult> Handle(GetUserByEmail query, CancellationToken cancellationToken)
    {
        query.NotBeNull();

        var identityUser = await _userManager.FindUserWithRoleByEmailAsync(query.Email);
        identityUser.NotBeNull(new IdentityUserNotFoundException(query.Email));

        var userDto = _mapper.Map<IdentityUserDto>(identityUser);

        return new GetUserByEmailResult(userDto);
    }
}

internal record GetUserByEmailResult(IdentityUserDto? UserIdentity);
