using System.Data;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;

namespace Ballast.Infrastructure.Repositories;

public class TodoRepository(IDatabase db) : ITodoRepository
{
    public Task<List<TodoItem>> GetAllAsync(Guid userId) =>
        db.QueryAsync(
            "SELECT Id, Title, IsDone, CreatedAt, UserId FROM TodoItems WHERE UserId = @UserId",
            new Dictionary<string, object> { ["@UserId"] = userId },
            MapRow);

    public async Task<TodoItem?> GetByIdAsync(Guid id)
    {
        var results = await db.QueryAsync(
            "SELECT Id, Title, IsDone, CreatedAt, UserId FROM TodoItems WHERE Id = @Id",
            new Dictionary<string, object> { ["@Id"] = id },
            MapRow);

        return results.Count > 0 ? results[0] : null;
    }

    public Task AddAsync(TodoItem item) =>
        db.ExecuteAsync(
            "INSERT INTO TodoItems (Id, Title, IsDone, CreatedAt, UserId) VALUES (@Id, @Title, @IsDone, @CreatedAt, @UserId)",
            new Dictionary<string, object>
            {
                ["@Id"] = item.Id,
                ["@Title"] = item.Title,
                ["@IsDone"] = item.IsDone,
                ["@CreatedAt"] = item.CreatedAt,
                ["@UserId"] = item.UserId
            });

    public Task UpdateAsync(TodoItem item) =>
        db.ExecuteAsync(
            "UPDATE TodoItems SET Title = @Title, IsDone = @IsDone WHERE Id = @Id",
            new Dictionary<string, object>
            {
                ["@Id"] = item.Id,
                ["@Title"] = item.Title,
                ["@IsDone"] = item.IsDone
            });

    public Task DeleteAsync(Guid id) =>
        db.ExecuteAsync(
            "DELETE FROM TodoItems WHERE Id = @Id",
            new Dictionary<string, object> { ["@Id"] = id });

    private static TodoItem MapRow(IDataRecord r) => new()
    {
        Id = r.GetGuid(0),
        Title = r.GetString(1),
        IsDone = r.GetBoolean(2),
        CreatedAt = r.GetDateTime(3),
        UserId = r.GetGuid(4)
    };
}
