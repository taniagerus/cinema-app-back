using cinema_app_back.Models;
using cinema_app_back.Services;
using System.Security.Claims;

namespace cinema_app_back.Middleware
{
    public class PostgresRoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PostgresRoleMiddleware> _logger;

        public PostgresRoleMiddleware(RequestDelegate next, ILogger<PostgresRoleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, PostgresRoleService postgresRoleService)
        {
            try
            {
                // Check if the user is authenticated
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    // Get the user's role from the claims
                    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
                    
                    if (roleClaim != null)
                    {
                        // Parse the role from the claim
                        if (Enum.TryParse<Role>(roleClaim.Value, out var role))
                        {
                            // Map the application role to a PostgreSQL role
                            var postgresRole = postgresRoleService.MapRoleToPostgresRole(role);
                            
                            // Store the PostgreSQL role in the HttpContext items for later use
                            context.Items["PostgresRole"] = postgresRole;
                            context.Items["ApplicationRole"] = role;
                            
                            _logger.LogInformation($"User authenticated with role: {role}, mapped to PostgreSQL role: {postgresRole}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not parse role from claim: {roleClaim.Value}");
                            // Default to guest role
                            context.Items["PostgresRole"] = postgresRoleService.MapRoleToPostgresRole(Role.Guest);
                            context.Items["ApplicationRole"] = Role.Guest;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Authenticated user has no role claim");
                        // Default to guest role
                        context.Items["PostgresRole"] = postgresRoleService.MapRoleToPostgresRole(Role.Guest);
                        context.Items["ApplicationRole"] = Role.Guest;
                    }
                }
                else
                {
                    _logger.LogInformation("User is not authenticated, using guest role");
                    // User is not authenticated, use guest role
                    context.Items["PostgresRole"] = postgresRoleService.MapRoleToPostgresRole(Role.Guest);
                    context.Items["ApplicationRole"] = Role.Guest;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PostgresRoleMiddleware");
                // Continue with the request even if there's an error
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    // Extension method to add the middleware to the application pipeline
    public static class PostgresRoleMiddlewareExtensions
    {
        public static IApplicationBuilder UsePostgresRoleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PostgresRoleMiddleware>();
        }
    }
}
