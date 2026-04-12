using Ballast.Application.Entities;
using Ballast.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace Ballast.Infrastructure.Repositories;

public class TodoRepository(string connectionString) : ITodoRepository
{
    public async Task<List<TodoItem>> GetAllAsync(Guid userId)
    {
        const string sql = "SELECT Id, Title, IsDone, CreatedAt, UserId FROM TodoItems WHERE UserId = @UserId";
        var results = new List<TodoItem>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            results.Add(MapRow(reader));

        return results;
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT Id, Title, IsDone, CreatedAt, UserId FROM TodoItems WHERE Id = @Id";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task AddAsync(TodoItem item)
    {
        const string sql = """
            INSERT INTO TodoItems (Id, Title, IsDone, CreatedAt, UserId)
            VALUES (@Id, @Title, @IsDone, @CreatedAt, @UserId)
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", item.Id);
        command.Parameters.AddWithValue("@Title", item.Title);
        command.Parameters.AddWithValue("@IsDone", item.IsDone);
        command.Parameters.AddWithValue("@CreatedAt", item.CreatedAt);
        command.Parameters.AddWithValue("@UserId", item.UserId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(TodoItem item)
    {
        const string sql = """
            UPDATE TodoItems
            SET Title = @Title, IsDone = @IsDone
            WHERE Id = @Id
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", item.Id);
        command.Parameters.AddWithValue("@Title", item.Title);
        command.Parameters.AddWithValue("@IsDone", item.IsDone);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM TodoItems WHERE Id = @Id";

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static TodoItem MapRow(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Title = reader.GetString(1),
        IsDone = reader.GetBoolean(2),
        CreatedAt = reader.GetDateTime(3),
        UserId = reader.GetGuid(4)
    };
}
