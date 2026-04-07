using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Users.Shared;

namespace Users.Infrastructure.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public CurrentUserState GetState()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        var isAuthenticated = principal?.Identity?.IsAuthenticated ?? false;

        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? principal?.FindFirst("sub")?.Value;
                        
        var userId = userIdClaim is not null && Guid.TryParse(userIdClaim, out var parsedUserId)
            ? parsedUserId
            : Guid.Empty;

        var email = principal?.FindFirst(ClaimTypes.Email)?.Value 
                   ?? principal?.FindFirst("email")?.Value 
                   ?? string.Empty;

        var role = principal?.FindFirst(ClaimTypes.Role)?.Value 
                  ?? principal?.FindFirst("role")?.Value 
                  ?? string.Empty;

        var managedFacilityIds = principal?.FindAll("ManagedFacilityId")
            .Where(c => Guid.TryParse(c.Value, out _))
            .Select(c => Guid.Parse(c.Value))
            .ToList() ?? [];

        return new CurrentUserState(userId, email, role, managedFacilityIds, isAuthenticated);
    }
}
