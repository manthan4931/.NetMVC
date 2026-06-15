using System.Security.Claims;

namespace ERP.Backend.Helpers
{
    public static class ClaimsHelper
    {
        public static Guid GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("userId") ?? user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedAccessException("User ID claim not found.");
            return Guid.Parse(claim.Value);
        }

        public static string GetUserRole(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.Role);
            return claim?.Value ?? string.Empty;
        }

        public static bool IsAdmin(ClaimsPrincipal user) =>
            GetUserRole(user) == "ADMIN";

        public static bool IsAdminOrManager(ClaimsPrincipal user)
        {
            var role = GetUserRole(user);
            return role == "ADMIN" || role == "PROJECT_MANAGER";
        }
    }
}
