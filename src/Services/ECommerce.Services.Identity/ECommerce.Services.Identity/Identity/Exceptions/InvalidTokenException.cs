using System.Security.Claims;
using BuildingBlocks.Core.Exception.Types;


namespace ECommerce.Services.Identity.Identity.Exceptions;

public class InvalidTokenException : BadRequestException
{
    public InvalidTokenException(ClaimsPrincipal? claimsPrincipal) : base("access_token is invalid!")
    {
    }
}
