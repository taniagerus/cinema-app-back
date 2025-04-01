using cinema_app_back.Data;
using cinema_app_back.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace cinema_app_back.Services
{
    public class PostgresRoleService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PostgresRoleService> _logger;

        public PostgresRoleService(IConfiguration configuration, ILogger<PostgresRoleService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Maps application Role enum to PostgreSQL role names
        /// </summary>
        public string MapRoleToPostgresRole(Role role)
        {
            return role switch
            {
                Role.Admin => "cinema_admin",
                Role.User => "cinema_user",
                Role.Guest => "cinema_guest",
                _ => "cinema_guest" // Default to guest for safety
            };
        }

        /// <summary>
        /// Gets a database connection with the appropriate PostgreSQL role
        /// </summary>
        public NpgsqlConnection GetConnectionWithRole(Role role)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var postgresRole = MapRoleToPostgresRole(role);

                // Create a connection with the base connection string
                var connection = new NpgsqlConnection(connectionString);
                
                // Open the connection
                connection.Open();
                
                // Set the role for this connection
                using (var cmd = new NpgsqlCommand($"SET ROLE {postgresRole};", connection))
                {
                    cmd.ExecuteNonQuery();
                    _logger.LogInformation($"Successfully set PostgreSQL role to: {postgresRole}");
                }
                
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting PostgreSQL role for {role}");
                throw;
            }
        }

        /// <summary>
        /// Creates a DbContextOptionsBuilder with the appropriate PostgreSQL role
        /// </summary>
        public DbContextOptionsBuilder<DataContext> GetOptionsWithRole(Role role)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var postgresRole = MapRoleToPostgresRole(role);
            
            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            
            // Configure to use Npgsql with the connection string
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                // Set the command timeout
                options.CommandTimeout(30);
                
                // Add connection initialization to set the role
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
            
            // Return the configured options builder
            return optionsBuilder;
        }

        /// <summary>
        /// Executes a database command with the appropriate PostgreSQL role
        /// </summary>
        public async Task<int> ExecuteCommandWithRoleAsync(Role role, string commandText, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnectionWithRole(role))
            {
                using (var command = new NpgsqlCommand(commandText, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Executes a query with the appropriate PostgreSQL role and returns a DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryWithRoleAsync(Role role, string queryText, params NpgsqlParameter[] parameters)
        {
            using (var connection = GetConnectionWithRole(role))
            {
                using (var command = new NpgsqlCommand(queryText, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }
    }
}
