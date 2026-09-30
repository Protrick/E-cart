using ECart.Application.DTOs.Auth;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Entities;
using ECart.Domain.Enums;
using ECart.Domain.Exceptions;

namespace ECart.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUsername = request.Username.Trim();

        var existingEmailUser = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingEmailUser != null)
        {
            throw new ConflictException("A user with this email address already exists.");
        }

        var existingUsernameUser = await _unitOfWork.Users.GetByUsernameAsync(normalizedUsername, cancellationToken);
        if (existingUsernameUser != null)
        {
            throw new ConflictException("A user with this username already exists.");
        }

        var role = UserRole.Customer;
        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
        {
            role = parsedRole;
        }

        var user = new User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = role,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user, out var expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var identifier = request.UsernameOrEmail.Trim();
        var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(identifier, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username/email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is deactivated. Please contact support.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user, out var expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new NotFoundException("User not found or account is deactivated.");
        }

        return MapToUserDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.Select(MapToUserDto).ToList();
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
