namespace StudentPortal.ServiceDefaults.Extensions;

using System.Security.Claims;


    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ??
                              user.FindFirst("sub");

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return Guid.Parse(userIdClaim.Value);
        }

        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user.Identity?.Name;
        }

        public static bool IsInRole(this ClaimsPrincipal user, string roleName)
        {
            return user.HasClaim(ClaimTypes.Role, roleName) ||
                   user.HasClaim("role", roleName);
        }
    }