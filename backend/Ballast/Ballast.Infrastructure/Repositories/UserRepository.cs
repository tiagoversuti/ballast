using System.Data;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;

namespace Ballast.Infrastructure.Repositories;

public class UserRepository(IDatabase db) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var results = await db.QueryAsync(
            "SELECT Id, Username, PasswordHash, CreatedAt FROM Users WHERE Username = @Username",
            new Dictionary<string, object> { ["@Username"] = username },
            MapRow);

        return results.Count > 0 ? results[0] : null;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var count = await db.ScalarIntAsync(
            "SELECT COUNT(1) FROM Users WHERE Username = @Username",
            new Dictionary<string, object> { ["@Username"] = username });

        return count > 0;
    }

    public Task AddAsync(User user) =>
        db.ExecuteAsync(
            "INSERT INTO Users (Id, Username, PasswordHash, CreatedAt) VALUES (@Id, @Username, @PasswordHash, @CreatedAt)",
            new Dictionary<string, object>
            {
                ["@Id"] = user.Id,
                ["@Username"] = user.Username,
                ["@PasswordHash"] = user.PasswordHash,
                ["@CreatedAt"] = user.CreatedAt
            });

    private static User MapRow(IDataRecord r) => new()
    {
        Id = r.GetGuid(0),
        Username = r.GetString(1),
        PasswordHash = r.GetString(2),
        CreatedAt = r.GetDateTime(3)
    };
}
