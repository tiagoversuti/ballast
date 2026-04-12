using Ballast.Application.Utilities;
using Microsoft.Data.SqlClient;

namespace Ballast.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await CreateTablesIfNotExistAsync(connection);
        await SeedDefaultUserIfNotExistAsync(connection);
    }

    private static async Task CreateTablesIfNotExistAsync(SqlConnection connection)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
            BEGIN
                CREATE TABLE Users
                (
                    Id           UNIQUEIDENTIFIER PRIMARY KEY,
                    Username     NVARCHAR(100)    NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(500)    NOT NULL,
                    CreatedAt    DATETIME2        NOT NULL
                )
            END

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TodoItems')
            BEGIN
                CREATE TABLE TodoItems
                (
                    Id        UNIQUEIDENTIFIER PRIMARY KEY,
                    Title     NVARCHAR(500)    NOT NULL,
                    IsDone    BIT              NOT NULL DEFAULT 0,
                    CreatedAt DATETIME2        NOT NULL,
                    UserId    UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id)
                )
            END
            """;

        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedDefaultUserIfNotExistAsync(SqlConnection connection)
    {
        const string checkSql = "SELECT COUNT(1) FROM Users";
        await using var checkCommand = new SqlCommand(checkSql, connection);
        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

        if (count > 0) return;

        const string insertSql = """
            INSERT INTO Users (Id, Username, PasswordHash, CreatedAt)
            VALUES (@Id, @Username, @PasswordHash, @CreatedAt)
            """;

        await using var insertCommand = new SqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("@Id", Guid.NewGuid());
        insertCommand.Parameters.AddWithValue("@Username", "user");
        insertCommand.Parameters.AddWithValue("@PasswordHash", PasswordHasher.Hash("password"));
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        await insertCommand.ExecuteNonQueryAsync();
    }
}
