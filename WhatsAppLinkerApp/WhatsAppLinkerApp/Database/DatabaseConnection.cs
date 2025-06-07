using System;
using System.Data;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatsAppLinkerApp.Database
{
    public class DatabaseConnection
    {
        private string _connectionString;
        private static bool _hasLoggedConnectionString = false;

        public DatabaseConnection()
        {
            try
            {
                // Build connection string piece by piece to identify issues
                var builder = new NpgsqlConnectionStringBuilder();
                builder.Host = "localhost";
                builder.Port = 15432;
                builder.Database = "whatsapp_bot_system";
                builder.Username = "postgres";
                builder.Password = GetDbPassword();
                // Don't add any extra parameters that might cause issues
                
                _connectionString = builder.ConnectionString;
                
                if (!_hasLoggedConnectionString)
                {
                    Console.WriteLine($"[DB] Connection string built successfully");
                    Console.WriteLine($"[DB] Host: {builder.Host}, Port: {builder.Port}, Database: {builder.Database}, User: {builder.Username}");
                    _hasLoggedConnectionString = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB_ERROR] Failed to build connection string: {ex.Message}");
                // Fallback to simple string
                _connectionString = $"Host=localhost;Port=5432;Database=whatsapp_bot_system;Username=postgres;Password={GetDbPassword()}";
            }
        }

        private string GetDbPassword()
        {
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres_admin_password";
            Console.WriteLine($"[DB] Using password: {password.Substring(0, Math.Min(3, password.Length))}***");
            return password;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            Console.WriteLine($"[DB] Executing QueryAsync: {sql.Substring(0, Math.Min(50, sql.Length))}...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QueryAsync<T>(sql, parameters);
                Console.WriteLine($"[DB] QueryAsync returned {result.Count()} rows");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB_ERROR] QueryAsync failed: {ex.Message}");
                throw;
            }
        }

        public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
        {
            Console.WriteLine($"[DB] Executing QuerySingleAsync: {sql.Substring(0, Math.Min(50, sql.Length))}...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleOrDefaultAsync<T?>(sql, parameters);
                Console.WriteLine($"[DB] QuerySingleAsync returned: {result != null}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB_ERROR] QuerySingleAsync failed: {ex.Message}");
                Console.WriteLine($"[DB_ERROR] Connection string: {_connectionString}");
                throw;
            }
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            Console.WriteLine($"[DB] Executing ExecuteAsync: {sql.Substring(0, Math.Min(50, sql.Length))}...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.ExecuteAsync(sql, parameters);
                Console.WriteLine($"[DB] ExecuteAsync affected {result} rows");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB_ERROR] ExecuteAsync failed: {ex.Message}");
                throw;
            }
        }
    }
}