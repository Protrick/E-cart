using ECart.Application.DTOs.Auth;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Interfaces.Services;
using ECart.Application.Services;
using ECart.Domain.Entities;
using ECart.Domain.Enums;
using ECart.Domain.Exceptions;
using Moq;
using Xunit;

namespace ECart.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _service = new AuthService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "johndoe",
            Email = "john@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "Admin"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(p => p.HashPassword(request.Password))
            .Returns("hashed_password_xyz");

        DateTime expiry = DateTime.UtcNow.AddDays(7);
        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(It.IsAny<User>(), out expiry))
            .Returns("jwt_token_sample");

        // Act
        var result = await _service.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt_token_sample", result.Token);
        Assert.Equal("johndoe", result.User.Username);
        Assert.Equal("john@example.com", result.User.Email);
        Assert.Equal("Admin", result.User.Role);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_InvalidRole_DefaultsToCustomer()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "customeruser",
            Email = "cust@example.com",
            Password = "SecurePassword123!",
            FirstName = "Cust",
            LastName = "User",
            Role = "NonExistentRole"
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
        DateTime expiry = DateTime.UtcNow;
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(It.IsAny<User>(), out expiry)).Returns("token");

        // Act
        var result = await _service.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("Customer", result.User.Role);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "johndoe",
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_UsernameAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("existinguser", It.IsAny<CancellationToken>())).ReturnsAsync(new User { Username = "existinguser" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsernameOrEmail = "johndoe",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = "hashed_pw",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(r => r.GetByUsernameOrEmailAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("Password123!", "hashed_pw"))
            .Returns(true);

        DateTime expiry = DateTime.UtcNow.AddDays(7);
        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(user, out expiry))
            .Returns("valid_token");

        // Act
        var result = await _service.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("valid_token", result.Token);
        Assert.Equal("johndoe", result.User.Username);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequestDto { UsernameOrEmail = "nobody", Password = "pwd" };
        _userRepositoryMock.Setup(r => r.GetByUsernameOrEmailAsync("nobody", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsernameOrEmail = "johndoe",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Username = "johndoe",
            PasswordHash = "correct_hashed_pw",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(r => r.GetByUsernameOrEmailAsync("johndoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("WrongPassword", "correct_hashed_pw"))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_DeactivatedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequestDto { UsernameOrEmail = "johndoe", Password = "Password123!" };
        var user = new User { Username = "johndoe", PasswordHash = "hashed_pw", IsActive = false };

        _userRepositoryMock.Setup(r => r.GetByUsernameOrEmailAsync("johndoe", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword("Password123!", "hashed_pw")).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "alice",
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            Role = UserRole.Customer,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetCurrentUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task GetCurrentUserAsync_UserNotFoundOrInactive_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCurrentUserAsync(userId, CancellationToken.None));

        // Inactive user
        var inactiveUser = new User { Id = userId, IsActive = false };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(inactiveUser);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCurrentUserAsync(userId, CancellationToken.None));
    }
}
