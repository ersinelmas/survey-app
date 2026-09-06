using Moq;
using SurveyApp.Application.DTOs.Auth;
using SurveyApp.Application.Interfaces;
using SurveyApp.Application.Services;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using Xunit;

namespace SurveyApp.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _passwordHasher.Object,
            _jwtTokenGenerator.Object,
            refreshTokenExpiryDays: 7);

        _jwtTokenGenerator.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-access-token");
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsTokens()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("new@user.com")).ReturnsAsync((User?)null);
        _passwordHasher.Setup(p => p.Hash("Password1!")).Returns("hashed");

        var result = await _sut.RegisterAsync(new RegisterRequest { Email = "new@user.com", Password = "Password1!" });

        Assert.Equal("new@user.com", result.Email);
        Assert.Equal("fake-access-token", result.Token);
        Assert.NotEmpty(result.RefreshToken);
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "new@user.com" && u.Role == UserRole.User)), Times.Once);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("taken@user.com")).ReturnsAsync(new User { Email = "taken@user.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.RegisterAsync(new RegisterRequest { Email = "taken@user.com", Password = "Password1!" }));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsTokens()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@user.com", PasswordHash = "hashed", Role = UserRole.Admin };
        _userRepository.Setup(r => r.GetByEmailAsync("user@user.com")).ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("correct-password", "hashed")).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequest { Email = "user@user.com", Password = "correct-password" });

        Assert.Equal("Admin", result.Role);
        Assert.Equal("fake-access-token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@user.com", PasswordHash = "hashed" };
        _userRepository.Setup(r => r.GetByEmailAsync("user@user.com")).ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("wrong-password", "hashed")).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "user@user.com", Password = "wrong-password" }));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorizedAccessException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("nobody@user.com")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = "nobody@user.com", Password = "whatever" }));
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_RevokesOldTokenAndIssuesNewOne()
    {
        var userId = Guid.NewGuid();
        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-refresh-token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = null
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("valid-refresh-token")).ReturnsAsync(existingToken);
        _userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, Email = "user@user.com" });

        var result = await _sut.RefreshAsync(new RefreshRequest { RefreshToken = "valid-refresh-token" });

        Assert.NotNull(existingToken.RevokedAt);
        Assert.NotEqual("valid-refresh-token", result.RefreshToken);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_ThrowsUnauthorizedAccessException()
    {
        var expiredToken = new RefreshToken
        {
            Token = "expired-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = null
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("expired-token")).ReturnsAsync(expiredToken);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.RefreshAsync(new RefreshRequest { RefreshToken = "expired-token" }));
    }

    [Fact]
    public async Task RefreshAsync_WithAlreadyRevokedToken_ThrowsUnauthorizedAccessException()
    {
        var revokedToken = new RefreshToken
        {
            Token = "revoked-token",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("revoked-token")).ReturnsAsync(revokedToken);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.RefreshAsync(new RefreshRequest { RefreshToken = "revoked-token" }));
    }

    [Fact]
    public async Task RefreshAsync_WithUnknownToken_ThrowsUnauthorizedAccessException()
    {
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("unknown-token")).ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.RefreshAsync(new RefreshRequest { RefreshToken = "unknown-token" }));
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_SetsRevokedAt()
    {
        var token = new RefreshToken { Token = "active-token", RevokedAt = null };
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("active-token")).ReturnsAsync(token);

        await _sut.LogoutAsync("active-token");

        Assert.NotNull(token.RevokedAt);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithUnknownToken_DoesNotThrow()
    {
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("missing-token")).ReturnsAsync((RefreshToken?)null);

        await _sut.LogoutAsync("missing-token");

        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
