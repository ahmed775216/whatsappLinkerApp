using NUnit.Framework;
using Npgsql;
using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WhatsAppLinkerApp.Tests
{
    [TestFixture]
    public class SqlFunctionTests
    {
        // IMPORTANT: Replace with your actual test database connection string.
        // It's highly recommended to use a dedicated test database.
        private const string TestConnectionString = "Host=localhost;Username=testuser;Password=testpassword;Database=testdb";

        private IDbConnection _dbConnection;

        [SetUp]
        public async Task Setup()
        {
            _dbConnection = new NpgsqlConnection(TestConnectionString);
            await ((NpgsqlConnection)_dbConnection).OpenAsync();

            // Optional: Recreate functions if they might change or for a clean slate.
            // This requires the SQL script to be accessible. For simplicity, we assume functions exist.
            // string schemaSql = await File.ReadAllTextAsync("path_to_your_schema.sql");
            // await _dbConnection.ExecuteAsync(schemaSql);

            // Clean up tables before each test to ensure isolation
            await _dbConnection.ExecuteAsync("DELETE FROM whitelisted_groups; DELETE FROM whitelisted_users; DELETE FROM api_sync_log; DELETE FROM bot_instances;");
        }

        [TearDown]
        public void Teardown()
        {
            _dbConnection?.Dispose();
        }

        private async Task<int> InsertBotInstance(string clientId)
        {
            return await _dbConnection.ExecuteScalarAsync<int>(
                "INSERT INTO bot_instances (client_id, name, created_at, updated_at) VALUES (@ClientId, @Name, NOW(), NOW()) RETURNING id;",
                new { ClientId = clientId, Name = "Test Bot " + clientId });
        }

        [Test]
        public async Task CleanupOldSyncLogs_DeletesOldLogs_ReturnsCorrectCount()
        {
            // Arrange
            var botInstanceId = await InsertBotInstance("test_client_cleanup");
            await _dbConnection.ExecuteAsync(
                "INSERT INTO api_sync_log (bot_instance_id, endpoint, status_code, started_at) VALUES " +
                "(@BotInstanceId, '/test1', 200, NOW() - INTERVAL '40 days'), " + // Old log
                "(@BotInstanceId, '/test2', 200, NOW() - INTERVAL '5 days');",    // Recent log
                new { BotInstanceId = botInstanceId });

            // Act
            var deletedCount = await _dbConnection.QuerySingleAsync<int>(
                "SELECT cleanup_old_sync_logs(@DaysToKeep);",
                new { DaysToKeep = 30 });

            // Assert
            Assert.AreEqual(1, deletedCount, "Should delete one old log.");

            var remainingLogs = await _dbConnection.QueryAsync(
                "SELECT * FROM api_sync_log WHERE bot_instance_id = @BotInstanceId;",
                new { BotInstanceId = botInstanceId });
            Assert.AreEqual(1, remainingLogs.Count(), "One recent log should remain.");
            Assert.IsTrue(remainingLogs.First().started_at > DateTime.UtcNow.AddDays(-30));
        }

        [Test]
        public async Task CleanupOldSyncLogs_NoOldLogs_ReturnsZero()
        {
            // Arrange
            var botInstanceId = await InsertBotInstance("test_client_no_old_logs");
            await _dbConnection.ExecuteAsync(
                "INSERT INTO api_sync_log (bot_instance_id, endpoint, status_code, started_at) VALUES " +
                "(@BotInstanceId, '/test_recent', 200, NOW() - INTERVAL '10 days');",
                new { BotInstanceId = botInstanceId });

            // Act
            var deletedCount = await _dbConnection.QuerySingleAsync<int>(
                "SELECT cleanup_old_sync_logs(@DaysToKeep);",
                new { DaysToKeep = 30 });

            // Assert
            Assert.AreEqual(0, deletedCount, "Should delete no logs if all are recent.");
        }

        [Test]
        public async Task GetBotInstanceId_ExistingClientId_ReturnsId()
        {
            // Arrange
            var clientId = "client123";
            var expectedId = await InsertBotInstance(clientId);

            // Act
            var actualId = await _dbConnection.QuerySingleOrDefaultAsync<int?>(
                "SELECT get_bot_instance_id(@ClientId);",
                new { ClientId = clientId });

            // Assert
            Assert.AreEqual(expectedId, actualId);
        }

        [Test]
        public async Task GetBotInstanceId_NonExistingClientId_ReturnsNull()
        {
            // Arrange
            var clientId = "non_existent_client";

            // Act
            var actualId = await _dbConnection.QuerySingleOrDefaultAsync<int?>(
                "SELECT get_bot_instance_id(@ClientId);",
                new { ClientId = clientId });

            // Assert
            Assert.IsNull(actualId, "Should return NULL for a non-existent client_id.");
        }

        [Test]
        public async Task IsWhitelisted_UserIsWhitelistedAndActive_ReturnsTrue()
        {
            // Arrange
            var botInstanceId = await InsertBotInstance("whitelist_user_active");
            var userJid = "user@example.com";
            await _dbConnection.ExecuteAsync(
                "INSERT INTO whitelisted_users (bot_instance_id, user_jid, api_active, created_at, updated_at) " +
                "VALUES (@BotInstanceId, @UserJid, TRUE, NOW(), NOW());",
                new { BotInstanceId = botInstanceId, UserJid = userJid });

            // Act
            var isWhitelisted = await _dbConnection.QuerySingleAsync<bool>(
                "SELECT is_whitelisted(@BotInstanceId, @Jid);",
                new { BotInstanceId = botInstanceId, Jid = userJid });

            // Assert
            Assert.IsTrue(isWhitelisted);
        }

        [Test]
        public async Task IsWhitelisted_UserIsWhitelistedButInactive_ReturnsFalse()
        {
            // Arrange
            var botInstanceId = await InsertBotInstance("whitelist_user_inactive");
            var userJid = "inactive_user@example.com";
            await _dbConnection.ExecuteAsync(
                "INSERT INTO whitelisted_users (bot_instance_id, user_jid, api_active, created_at, updated_at) " +
                "VALUES (@BotInstanceId, @UserJid, FALSE, NOW(), NOW());",
                new { BotInstanceId = botInstanceId, UserJid = userJid });

            // Act
            var isWhitelisted = await _dbConnection.QuerySingleAsync<bool>(
                "SELECT is_whitelisted(@BotInstanceId, @Jid);",
                new { BotInstanceId = botInstanceId, Jid = userJid });

            // Assert
            Assert.IsFalse(isWhitelisted);
        }

        [Test]
        public async Task IsWhitelisted_GroupIsWhitelistedAndActive_ReturnsTrue()
        {
            // Arrange
            var botInstanceId = await InsertBotInstance("whitelist_group_active");
            var groupJid = "group@example.com";
            await _dbConnection.ExecuteAsync(
                "INSERT INTO whitelisted_groups (bot_instance_id, group_jid, is_active, created_at, updated_at) " +
                "VALUES (@BotInstanceId, @GroupJid, TRUE, NOW(), NOW());",
                new { BotInstanceId = botInstanceId, GroupJid = groupJid });

            // Act
            var isWhitelisted = await _dbConnection.QuerySingleAsync<bool>(
                "SELECT is_whitelisted(@BotInstanceId, @Jid);",
                new { BotInstanceId = botInstanceId, Jid = groupJid });

            // Assert
            Assert.IsTrue(isWhitelisted);
        }

        [Test]
        public async Task IsWhitelisted_JidNotWhitelisted_ReturnsFalse()
        {
            // Arrange
            var botInstanceId = await InsertBotInstance("whitelist_not_found");
            var jid = "unknown@example.com";

            // Act
            var isWhitelisted = await _dbConnection.QuerySingleAsync<bool>(
                "SELECT is_whitelisted(@BotInstanceId, @Jid);",
                new { BotInstanceId = botInstanceId, Jid = jid });

            // Assert
            Assert.IsFalse(isWhitelisted);
        }
    }
}