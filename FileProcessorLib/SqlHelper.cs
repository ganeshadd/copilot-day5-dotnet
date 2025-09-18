using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileProcessorLib
{
    /// <summary>
    /// A helper class for database operations with SQL injection protection
    /// </summary>
    public class SqlHelper : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;
        private bool _disposed = false;
        private readonly ILogger<SqlHelper>? _logger;

        /// <summary>
        /// Configuration key for the database connection string
        /// </summary>
        public const string ConnectionStringKey = "Database:ConnectionString";

        /// <summary>
        /// Creates a new SqlHelper using a connection string from configuration
        /// </summary>
        /// <param name="configuration">IConfiguration instance containing connection strings</param>
        /// <param name="logger">Optional logger for SQL operations</param>
        public SqlHelper(IConfiguration configuration, ILogger<SqlHelper>? logger = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string? connectionString = configuration[ConnectionStringKey];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Connection string with key '{ConnectionStringKey}' not found in configuration");

            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new SqlHelper with an explicit connection string
        /// Should only be used for testing or when configuration is not available
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="logger">Optional logger for SQL operations</param>
        public SqlHelper(string connectionString, ILogger<SqlHelper>? logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger;
        }

        /// <summary>
        /// Executes a query safely using parameterized SQL to prevent injection attacks
        /// </summary>
        /// <param name="userId">User ID to search for</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Count of matching records</returns>
        public int ExecuteSafeQuery(string userId, string searchTerm)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Null search term is acceptable - we'll just make it an empty string
            searchTerm = searchTerm ?? string.Empty;

            try
            {
                string query = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId AND Name LIKE @SearchTerm";
                _logger?.LogDebug("Executing parameterized query for user {UserId}", userId);

                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    // Properly parametrize all user inputs
                    command.Parameters.Add("@UserId", SqlDbType.VarChar, 50).Value = userId;
                    command.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 100).Value = $"%{searchTerm}%";

                    // Set command timeout
                    command.CommandTimeout = 30; // seconds

                    // Open connection and execute query
                    connection.Open();
                    var result = command.ExecuteScalar();

                    // Properly handle null result
                    return result == DBNull.Value || result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "SQL error executing safe query: {Message}", ex.Message);
                throw new DatabaseOperationException("Error executing database query", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error in ExecuteSafeQuery: {Message}", ex.Message);
                throw new DatabaseOperationException("Unexpected error during database operation", ex);
            }
        }

        /// <summary>
        /// Unsafe method for demonstration purposes only - DO NOT USE IN PRODUCTION
        /// This method is vulnerable to SQL injection attacks
        /// </summary>
        /// <param name="userId">User ID to search for</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Count of matching records</returns>
        [Obsolete("This method is unsafe and vulnerable to SQL injection. Use ExecuteSafeQuery instead.")]
        public int ExecuteUnsafeQuery(string userId, string searchTerm)
        {
            // WARNING: This is vulnerable to SQL injection!
            _logger?.LogWarning("UNSAFE SQL QUERY BEING EXECUTED - SQL INJECTION RISK!");

            try
            {
                string query = $"SELECT COUNT(*) FROM Users WHERE UserId = '{userId}' AND Name LIKE '%{searchTerm}%'";

                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    return result == DBNull.Value || result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (SqlException ex)
            {
                _logger?.LogError(ex, "SQL error executing unsafe query: {Message}", ex.Message);
                throw new DatabaseOperationException("Error executing database query", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error in ExecuteUnsafeQuery: {Message}", ex.Message);
                throw new DatabaseOperationException("Unexpected error during database operation", ex);
            }
        }

        /// <summary>
        /// Disposes of database resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases database resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }

                _connection = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~SqlHelper()
        {
            Dispose(false);
        }
    }
}