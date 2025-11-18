using DocumentManagement.Domain.Enums;
using System.Security.Claims;

namespace DocumentManagement.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User email not found in token");
        }

        public static UserRole GetUserRole(this ClaimsPrincipal principal)
        {
            var roleStr = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleStr) || !Enum.TryParse<UserRole>(roleStr, out var role))
            {
                throw new UnauthorizedAccessException("User role not found in token");
            }

            return role;
        }

        public static string GetIpAddress(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
