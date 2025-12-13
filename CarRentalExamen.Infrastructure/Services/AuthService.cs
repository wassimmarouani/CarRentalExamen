using CarRentalExamen.Core.DTOs.Auth;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Interfaces;
using CarRentalExamen.Core.Interfaces.Services;
using CarRentalExamen.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Authentication service - handles login, register, and user operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var customerId = await GetCustomerIdAsync(user.Id);
        return CreateAuthResponse(user, _jwtTokenService.GenerateToken(user), customerId);
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Email = request.Email,
            Role = request.Role
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return CreateAuthResponse(user, _jwtTokenService.GenerateToken(user), null);
    }

    public async Task<AuthResponseDto?> RegisterCustomerAsync(RegisterCustomerRequestDto request)
    {
        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Email = request.Email,
            Role = UserRole.Customer
        };

        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            CinOrPassport = request.CinOrPassport,
            LicenseNumber = request.LicenseNumber,
            Phone = request.Phone,
            Email = request.Email,
            User = user
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var customerId = customer.Id;
        return CreateAuthResponse(user, _jwtTokenService.GenerateToken(user), customerId);
    }

    public async Task<AuthResponseDto?> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null) return null;

        var customerId = await GetCustomerIdAsync(user.Id);
        return CreateAuthResponse(user, string.Empty, customerId);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _unitOfWork.Users.Query().AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _unitOfWork.Users.Query().AnyAsync(u => u.Email == email);
    }

    public async Task<bool> AnyUsersExistAsync()
    {
        return await _unitOfWork.Users.Query().AnyAsync();
    }

    private AuthResponseDto CreateAuthResponse(User user, string token, int? customerId)
    {
        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            CustomerId = customerId,
            Username = user.Username,
            Role = user.Role.ToString(),
            Email = user.Email
        };
    }

    private async Task<int?> GetCustomerIdAsync(int userId)
    {
        return await _unitOfWork.Customers.Query()
            .Where(c => c.UserId == userId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }
}
