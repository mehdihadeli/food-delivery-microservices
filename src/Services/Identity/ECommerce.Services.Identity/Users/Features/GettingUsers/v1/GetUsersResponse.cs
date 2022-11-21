using BuildingBlocks.Core.CQRS.Queries;
using ECommerce.Services.Identity.Users.Dtos;
using ECommerce.Services.Identity.Users.Dtos.v1;

namespace ECommerce.Services.Identity.Users.Features.GettingUsers.v1;

public record GetUsersResponse(ListResultModel<IdentityUserDto> IdentityUsers);
