using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Ballast.Application.DTOs;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;
using Ballast.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Ballast.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-for-testing-purposes-only-32c",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        _sut = new AuthService(_userRepositoryMock.Object, config);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAvailable_ReturnsToken()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("alice")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync(new RegisterDto("alice", "password123"));

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAvailable_SavesUserWithHashedPassword()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("alice")).ReturnsAsync(false);

        User? savedUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => savedUser = u)
            .Returns(Task.CompletedTask);

        await _sut.RegisterAsync(new RegisterDto("alice", "password123"));

        Assert.NotNull(savedUser);
        Assert.Equal("alice", savedUser.Username);
        Assert.NotEqual("password123", savedUser.PasswordHash);
        Assert.Contains(".", savedUser.PasswordHash);
        Assert.NotEqual(Guid.Empty, savedUser.Id);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameTaken_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("alice")).ReturnsAsync(true);

        var result = await _sut.RegisterAsync(new RegisterDto("alice", "password123"));

        Assert.Null(result);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_TokenContainsUserId()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("alice")).ReturnsAsync(false);

        User? savedUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => savedUser = u)
            .Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync(new RegisterDto("alice", "password123"));

        Assert.NotNull(result);
        Assert.NotNull(savedUser);
        var claims = ParseToken(result.Token);
        var subject = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        Assert.Equal(savedUser.Id.ToString(), subject);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            PasswordHash = HashPassword("password123"),
            CreatedAt = DateTime.UtcNow
        };
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginDto("alice", "password123"));

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_TokenContainsUserId()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "alice",
            PasswordHash = HashPassword("password123"),
            CreatedAt = DateTime.UtcNow
        };
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginDto("alice", "password123"));

        Assert.NotNull(result);
        var claims = ParseToken(result.Token);
        var subject = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        Assert.Equal(userId.ToString(), subject);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginDto("alice", "password123"));

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            PasswordHash = HashPassword("correctpassword"),
            CreatedAt = DateTime.UtcNow
        };
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginDto("alice", "wrongpassword"));

        Assert.Null(result);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static IEnumerable<Claim> ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }
}
