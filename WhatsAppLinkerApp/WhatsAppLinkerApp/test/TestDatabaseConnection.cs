// TestDatabaseConnection.cs
using System;
using Npgsql;

class TestDatabaseConnection
{
    static void Test()
    {
        Console.WriteLine("Testing database connection...");
        
        try
        {
            var connString = "Host=localhost;Port=15432;Database=whatsapp_bot_system;Username=postgres;Password=postgres_admin_password;";
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            
            Console.WriteLine("Connection successful!");
            
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM bot_instances", conn);
            var count = cmd.ExecuteScalar();
            Console.WriteLine($"Bot instances in database: {count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
}