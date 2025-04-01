using cinema_app_back.Data;
using cinema_app_back.Models;
using cinema_app_back.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cinema_app_back.Controllers
{
    public abstract class RoleBasedController : ControllerBase
    {
        protected readonly RoleBasedDbContextFactory _dbContextFactory;
        protected readonly ILogger<RoleBasedController> _logger;

        protected RoleBasedController(RoleBasedDbContextFactory dbContextFactory, ILogger<RoleBasedController> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the user's role from the HttpContext
        /// </summary>
        protected Role GetUserRole()
        {
            // Check if the role was already determined by the middleware
            if (HttpContext.Items.TryGetValue("ApplicationRole", out var roleObj) && roleObj is Role role)
            {
                return role;
            }

            // If not, determine it from the claims
            if (User.Identity?.IsAuthenticated == true)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role);
                if (roleClaim != null && Enum.TryParse<Role>(roleClaim.Value, out var claimRole))
                {
                    return claimRole;
                }
            }

            // Default to guest
            return Role.Guest;
        }

        /// <summary>
        /// Gets a DataContext with the appropriate PostgreSQL role based on the user's role
        /// </summary>
        protected DataContext GetRoleBasedDbContext()
        {
            var role = GetUserRole();
            _logger.LogInformation($"Creating DbContext with role: {role}");
            return _dbContextFactory.CreateDbContext(role);
        }
    }
}
