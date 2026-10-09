using System.Net;
using System.Security.Claims;
using Squirrel.Core.Exception;

namespace FoodDelivery.Services.Identity.Identity.Exceptions;

public class InvalidTokenException : AppException
{
    public InvalidTokenException(ClaimsPrincipal? claimsPrincipal)
        : base("access_token is invalid!") { }
}
