using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Identity.Users.Dtos;

namespace ECommerce.Services.Identity.Users.Features.GettingUsers;

public record GetUsersResponse(ListResultModel<IdentityUserDto> IdentityUsers);
