using System;
using System.Data;
using System.IO; // Required for Stream
using System.Reflection; // Required for Assembly
using Npgsql;
using Dapper;
using Newtonsoft.Json.Linq; // Required for JObject
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatsAppLinkerApp.Database
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;
        private static bool _hasLoggedConnectionString = false;

        public DatabaseConnection()
        {
            try
            {
                // Load configuration from the embedded resource
                JObject config = LoadConfiguration();
                _connectionString = config["ConnectionStrings"]?["PostgreSQL"]?.ToString() ??
                                    throw new InvalidOperationException("PostgreSQL connection string not found in embedded appSettings.json.");

                if (!_hasLoggedConnectionString)
                {
                    // For security, let's parse the connection string to log it safely without the password.
                    var builder = new NpgsqlConnectionStringBuilder(_connectionString);
                    Console.WriteLine($"[DB] Connection string loaded successfully from embedded resource.");
                    Console.WriteLine($"[DB] Host: {builder.Host}, Port: {builder.Port}, Database: {builder.Database}, User: {builder.Username}");
                    _hasLoggedConnectionString = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB_ERROR] CRITICAL: Failed to load or parse embedded configuration: {ex.Message}");
                // Set a non-functional connection string to ensure failures are obvious
                _connectionString = string.Empty;
                // Optionally, re-throw the exception to halt the application if config is essential
                throw;
            }
        }

        private JObject LoadConfiguration()
        {
            // Get the current assembly (the .exe file itself)
            var assembly = Assembly.GetExecutingAssembly();

            // The resource name is calculated as <RootNamespace>.<FileName>
            // Your root namespace is "WhatsAppLinkerApp"
            string resourceName = "WhatsAppLinkerApp.appSettings.json";

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Ensure its 'Build Action' is set to 'Embedded Resource' in the project file.");
                }
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    return JObject.Parse(json);
                }
            }
        }

        // The QueryAsync, QuerySingleAsync, and ExecuteAsync methods remain unchanged.
        // I am including them here for completeness of the class.

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new InvalidOperationException("Database connection is not configured.");
            Console.WriteLine($"[DB] Executing QueryAsync: {sql.Substring(0, Math.Min(50, sql.Length))}...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QueryAsync<T>(sql, parameters);
                // Console.WriteLine($"[DB] QueryAsync returned {result.Count()} rows");
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
            if (string.IsNullOrEmpty(_connectionString)) throw new InvalidOperationException("Database connection is not configured.");
            Console.WriteLine($"[DB] Executing QuerySingleAsync: {sql.Substring(0, Math.Min(50, sql.Length))}...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.QuerySingleOrDefaultAsync<T?>(sql, parameters);
                // Console.WriteLine($"[DB] QuerySingleAsync returned: {(result != null)}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB_ERROR] QuerySingleAsync failed: {ex.Message}");
                // Console.WriteLine($"[DB_ERROR] Connection string used (password omitted): {new NpgsqlConnectionStringBuilder(_connectionString) { Password = \"\" }.ConnectionString}");
                throw;
            }
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new InvalidOperationException("Database connection is not configured.");
            Console.WriteLine($"[DB] Executing ExecuteAsync: {sql.Substring(0, Math.Min(50, sql.Length))}...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var result = await connection.ExecuteAsync(sql, parameters);
                // Console.WriteLine($"[DB] ExecuteAsync affected {result} rows");
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