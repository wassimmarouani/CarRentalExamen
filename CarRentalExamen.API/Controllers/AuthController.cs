using CarRentalExamen.Core.DTOs.Auth;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalExamen.API.Controllers;

/// <summary>
/// Authentication controller - thin controller that delegates to AuthService
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized("Invalid credentials");
        }
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        if (await _authService.UsernameExistsAsync(request.Username))
        {
            return Conflict("Username already exists.");
        }

        if (await _authService.EmailExistsAsync(request.Email))
        {
            return Conflict("Email already exists.");
        }

        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.GetUserByIdAsync(userId);
        if (result is null)
        {
            return NotFound("User not found");
        }
        return Ok(result);
    }
}
