using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.DTOs.Auth;

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Agent;
}
