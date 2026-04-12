using Ballast.Application.DTOs;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;
using Ballast.Application.Services;
using Moq;
using Xunit;

namespace Ballast.Tests.Services;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repositoryMock = new();
    private readonly TodoService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public TodoServiceTests()
    {
        _service = new TodoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTodosForUser()
    {
        var items = new List<TodoItem>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = _userId },
            new() { Id = Guid.NewGuid(), Title = "Task 2", IsDone = true,  CreatedAt = DateTime.UtcNow, UserId = _userId }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(_userId)).ReturnsAsync(items);

        var result = await _service.GetAllAsync(_userId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Task 1", result[0].Title);
        Assert.Equal("Task 2", result[1].Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFoundAndOwned_ReturnsTodo()
    {
        var id = Guid.NewGuid();
        var item = new TodoItem { Id = id, Title = "Task", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = _userId };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);

        var result = await _service.GetByIdAsync(id, _userId);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Task", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TodoItem?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), _userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByOtherUser_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var item = new TodoItem { Id = id, Title = "Task", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);

        var result = await _service.GetByIdAsync(id, _userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsTodo()
    {
        var dto = new CreateTodoDto("New Task");
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto, _userId);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("New Task", result.Title);
        Assert.False(result.IsDone);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<TodoItem>(t => t.Title == "New Task" && t.UserId == _userId)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenFoundAndOwned_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var item = new TodoItem { Id = id, Title = "Old", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = _userId };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(id, new UpdateTodoDto("New", true), _userId);

        Assert.True(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<TodoItem>(t => t.Title == "New" && t.IsDone)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TodoItem?)null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateTodoDto("New", true), _userId);

        Assert.False(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByOtherUser_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var item = new TodoItem { Id = id, Title = "Task", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);

        var result = await _service.UpdateAsync(id, new UpdateTodoDto("New", true), _userId);

        Assert.False(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFoundAndOwned_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var item = new TodoItem { Id = id, Title = "Task", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = _userId };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);
        _repositoryMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(id, _userId);

        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TodoItem?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid(), _userId);

        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByOtherUser_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var item = new TodoItem { Id = id, Title = "Task", IsDone = false, CreatedAt = DateTime.UtcNow, UserId = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);

        var result = await _service.DeleteAsync(id, _userId);

        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
