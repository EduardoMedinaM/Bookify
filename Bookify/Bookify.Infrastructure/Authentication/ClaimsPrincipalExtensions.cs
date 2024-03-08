using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Bookify.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userId, out var parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User identifier is unavailable");
    }

    public static string GetIdentityId(this ClaimsPrincipal? principal)
    {
        /*
         * The sub claim "sub": "28140b14-f981-47b5-abbe-d98643a9dab5" is 
         * transformed in nameIdentifier
         **/
        return principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
               throw new ApplicationException("User identity is unavailable");
    }
}
