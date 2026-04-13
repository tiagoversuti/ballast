using System.Data;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;
using Ballast.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace Ballast.Tests.Repositories;

public class TodoRepositoryTests
{
    private readonly Mock<IDatabase> _dbMock = new();
    private readonly TodoRepository _sut;

    public TodoRepositoryTests()
    {
        _sut = new TodoRepository(_dbMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_CallsQueryWithCorrectUserId()
    {
        var userId = Guid.NewGuid();
        _dbMock
            .Setup(d => d.QueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<Func<IDataRecord, TodoItem>>()))
            .ReturnsAsync([]);

        await _sut.GetAllAsync(userId);

        _dbMock.Verify(d => d.QueryAsync(
            It.Is<string>(s => s.Contains("WHERE UserId")),
            It.Is<IReadOnlyDictionary<string, object>>(p => p["@UserId"].Equals(userId)),
            It.IsAny<Func<IDataRecord, TodoItem>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedItems()
    {
        var userId = Guid.NewGuid();
        var expected = new List<TodoItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = userId },
            new() { Id = Guid.NewGuid(), Title = "Task 2", IsDone = true,  CreatedAt = DateTime.UtcNow, UserId = userId }
        };
        _dbMock
            .Setup(d => d.QueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<Func<IDataRecord, TodoItem>>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetAllAsync(userId);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRowReturned_ReturnsTodo()
    {
        var id = Guid.NewGuid();
        var todo = new TodoItem { Id = id, Title = "Task", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = Guid.NewGuid() };
        _dbMock
            .Setup(d => d.QueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<Func<IDataRecord, TodoItem>>()))
            .ReturnsAsync([todo]);

        var result = await _sut.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _dbMock.Verify(d => d.QueryAsync(
            It.IsAny<string>(),
            It.Is<IReadOnlyDictionary<string, object>>(p => p["@Id"].Equals(id)),
            It.IsAny<Func<IDataRecord, TodoItem>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNoRow_ReturnsNull()
    {
        _dbMock
            .Setup(d => d.QueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<Func<IDataRecord, TodoItem>>()))
            .ReturnsAsync([]);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_CallsExecuteWithAllFields()
    {
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = "New Task",
            IsDone = false,
            CreatedAt = DateTime.UtcNow,
            UserId = Guid.NewGuid()
        };

        await _sut.AddAsync(item);

        _dbMock.Verify(d => d.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<IReadOnlyDictionary<string, object>>(p =>
                p["@Id"].Equals(item.Id) &&
                p["@Title"].Equals(item.Title) &&
                p["@IsDone"].Equals(item.IsDone) &&
                p["@CreatedAt"].Equals(item.CreatedAt) &&
                p["@UserId"].Equals(item.UserId))),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsExecuteWithCorrectParams()
    {
        var item = new TodoItem { Id = Guid.NewGuid(), Title = "Updated", IsDone = true, CreatedAt = DateTime.UtcNow, UserId = Guid.NewGuid() };

        await _sut.UpdateAsync(item);

        _dbMock.Verify(d => d.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<IReadOnlyDictionary<string, object>>(p =>
                p["@Id"].Equals(item.Id) &&
                p["@Title"].Equals(item.Title) &&
                p["@IsDone"].Equals(item.IsDone))),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsExecuteWithId()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id);

        _dbMock.Verify(d => d.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<IReadOnlyDictionary<string, object>>(p => p["@Id"].Equals(id))),
            Times.Once);
    }
}
