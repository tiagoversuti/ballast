using System.Security.Claims;
using Ballast.Api.Controllers;
using Ballast.Application.DTOs;
using Ballast.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ballast.Tests.Controllers;

public class TodosControllerTests
{
    private readonly Mock<ITodoService> _serviceMock = new();
    private readonly TodosController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public TodosControllerTests()
    {
        _controller = new TodosController(_serviceMock.Object)
        {
            ControllerContext = BuildControllerContext(_userId)
        };
    }

    private static ControllerContext BuildControllerContext(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task GetAll_Returns200WithTodos()
    {
        var todos = new List<TodoDto>
        {
            new(Guid.NewGuid(), "Task 1", false, DateTime.UtcNow),
            new(Guid.NewGuid(), "Task 2", true,  DateTime.UtcNow)
        };
        _serviceMock.Setup(s => s.GetAllAsync(_userId)).ReturnsAsync(todos);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(todos, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenFound_Returns200()
    {
        var id = Guid.NewGuid();
        var todo = new TodoDto(id, "Task", false, DateTime.UtcNow);
        _serviceMock.Setup(s => s.GetByIdAsync(id, _userId)).ReturnsAsync(todo);

        var result = await _controller.GetById(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(todo, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), _userId)).ReturnsAsync((TodoDto?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Returns201WithTodo()
    {
        var dto = new CreateTodoDto("New Task");
        var created = new TodoDto(Guid.NewGuid(), "New Task", false, DateTime.UtcNow);
        _serviceMock.Setup(s => s.CreateAsync(dto, _userId)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(created, createdResult.Value);
        Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
    }

    [Fact]
    public async Task Update_WhenFound_Returns204()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateTodoDto>(), _userId)).ReturnsAsync(true);

        var result = await _controller.Update(id, new UpdateTodoDto("Updated", true));

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateTodoDto>(), _userId)).ReturnsAsync(false);

        var result = await _controller.Update(Guid.NewGuid(), new UpdateTodoDto("Updated", true));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenFound_Returns204()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, _userId)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), _userId)).ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
