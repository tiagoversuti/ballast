using Ballast.Api.Controllers;
using Ballast.Application.DTOs;
using Ballast.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ballast.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WhenUsernameAvailable_Returns200WithToken()
    {
        var response = new AuthResponseDto("jwt-token");
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync(response);

        var result = await _controller.Register(new RegisterDto("alice", "password"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task Register_WhenUsernameTaken_Returns409()
    {
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.Register(new RegisterDto("alice", "password"));

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var response = new AuthResponseDto("jwt-token");
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(response);

        var result = await _controller.Login(new LoginDto("alice", "password"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.Login(new LoginDto("alice", "wrongpassword"));

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
