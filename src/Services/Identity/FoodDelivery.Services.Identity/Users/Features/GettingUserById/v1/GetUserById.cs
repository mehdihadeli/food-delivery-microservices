using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Extensions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Users.Features.GettingUserById.v1;

public record GetUserById(Guid Id) : IQuery<GetUserByIdResult>
{
    public static GetUserById Of(Guid id) => new GetUserById(id.NotBeEmpty());
}

public class GetUserByIdValidator : AbstractValidator<GetUserById>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("InternalCommandId is required.");
    }
}

public class GetUserByIdHandler(UserManager<ApplicationUser> userManager)
    : IQueryHandler<GetUserById, GetUserByIdResult>
{
    public async ValueTask<GetUserByIdResult> Handle(GetUserById query, CancellationToken cancellationToken)
    {
        query.NotBeNull();

        var identityUser = await userManager.FindUserWithRoleByIdAsync(query.Id);
        identityUser.NotBeNull(new IdentityUserNotFoundException(query.Id));

        var identityUserDto = identityUser.ToIdentityUserDto();

        return new GetUserByIdResult(identityUserDto);
    }
}

public record GetUserByIdResult(IdentityUserDto IdentityUser);
