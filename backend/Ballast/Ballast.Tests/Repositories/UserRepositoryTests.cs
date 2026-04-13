using System.Data;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;
using Ballast.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace Ballast.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<IDatabase> _dbMock = new();
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _sut = new UserRepository(_dbMock.Object);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenFound_ReturnsMappedUser()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "alice", PasswordHash = "salt.hash", CreatedAt = DateTime.UtcNow };
        _dbMock
            .Setup(d => d.QueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<Func<IDataRecord, User>>()))
            .ReturnsAsync([user]);

        var result = await _sut.GetByUsernameAsync("alice");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenNotFound_ReturnsNull()
    {
        _dbMock
            .Setup(d => d.QueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<Func<IDataRecord, User>>()))
            .ReturnsAsync([]);

        var result = await _sut.GetByUsernameAsync("nobody");

        Assert.Null(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_WhenCountIsOne_ReturnsTrue()
    {
        _dbMock
            .Setup(d => d.ScalarIntAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _sut.UsernameExistsAsync("alice");

        Assert.True(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_WhenCountIsZero_ReturnsFalse()
    {
        _dbMock
            .Setup(d => d.ScalarIntAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
            .ReturnsAsync(0);

        var result = await _sut.UsernameExistsAsync("nobody");

        Assert.False(result);
    }

    [Fact]
    public async Task AddAsync_CallsExecuteWithAllUserFields()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "bob",
            PasswordHash = "salt.hash",
            CreatedAt = DateTime.UtcNow
        };

        await _sut.AddAsync(user);

        _dbMock.Verify(d => d.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<IReadOnlyDictionary<string, object>>(p =>
                p["@Id"].Equals(user.Id) &&
                p["@Username"].Equals(user.Username) &&
                p["@PasswordHash"].Equals(user.PasswordHash) &&
                p["@CreatedAt"].Equals(user.CreatedAt))),
            Times.Once);
    }
}
