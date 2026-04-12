using Ballast.Application.Entities;
using Ballast.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace Ballast.Infrastructure.Repositories;

public class UserRepository(string connectionString) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = "SELECT Id, Username, PasswordHash, CreatedAt FROM Users WHERE Username = @Username";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", username);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Username", username);
        await connection.OpenAsync();
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task AddAsync(User user)
    {
        const string sql = """
            INSERT INTO Users (Id, Username, PasswordHash, CreatedAt)
            VALUES (@Id, @Username, @PasswordHash, @CreatedAt)
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static User MapRow(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Username = reader.GetString(1),
        PasswordHash = reader.GetString(2),
        CreatedAt = reader.GetDateTime(3)
    };
}
