using cinema_app_back.Data;
using cinema_app_back.Models;
using Microsoft.EntityFrameworkCore;

namespace cinema_app_back.Services
{
    public class RoleBasedDbContextFactory
    {
        private readonly PostgresRoleService _postgresRoleService;
        private readonly ILogger<RoleBasedDbContextFactory> _logger;

        public RoleBasedDbContextFactory(PostgresRoleService postgresRoleService, ILogger<RoleBasedDbContextFactory> logger)
        {
            _postgresRoleService = postgresRoleService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a DataContext with the appropriate PostgreSQL role based on the user's application role
        /// </summary>
        public DataContext CreateDbContext(Role role)
        {
            try
            {
                _logger.LogInformation($"Creating DbContext with role: {role}");
                
                // Get options with the appropriate role
                var options = _postgresRoleService.GetOptionsWithRole(role);
                
                // Create and return the context
                var context = new DataContext(options.Options);
                
                // Set the role for this connection when it's opened
                context.Database.OpenConnection();
                var connection = context.Database.GetDbConnection() as Npgsql.NpgsqlConnection;
                
                if (connection != null)
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"SET ROLE {_postgresRoleService.MapRoleToPostgresRole(role)};";
                        cmd.ExecuteNonQuery();
                        _logger.LogInformation($"Successfully set PostgreSQL role to: {_postgresRoleService.MapRoleToPostgresRole(role)}");
                    }
                }
                
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating DbContext with role: {role}");
                throw;
            }
        }
    }
}
