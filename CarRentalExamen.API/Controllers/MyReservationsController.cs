using System.Security.Claims;
using CarRentalExamen.Core.DTOs.Reservations;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalExamen.API.Controllers;

[ApiController]
[Route("api/my/reservations")]
[Authorize(Roles = "Customer")]
public class MyReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ICustomerService _customerService;

    public MyReservationsController(IReservationService reservationService, ICustomerService customerService)
    {
        _reservationService = reservationService;
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDetailDto>>> GetMine()
    {
        var customerId = await GetCustomerIdForCurrentUser();
        if (customerId is null) return NotFound("Customer profile not found.");

        var reservations = await _reservationService.GetByCustomerAsync(customerId.Value);
        return Ok(reservations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDetailDto>> GetById(int id)
    {
        var customerId = await GetCustomerIdForCurrentUser();
        if (customerId is null) return NotFound("Customer profile not found.");

        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation is null || reservation.CustomerId != customerId.Value)
        {
            return NotFound();
        }
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDetailDto>> Create([FromBody] ReservationCustomerCreateDto dto)
    {
        var customerId = await GetCustomerIdForCurrentUser();
        if (customerId is null) return NotFound("Customer profile not found.");

        var (success, error, reservation) = await _reservationService.CreateForCustomerAsync(dto, customerId.Value);
        if (!success)
        {
            return error!.Contains("overlapping") ? Conflict(error) : BadRequest(error);
        }
        return CreatedAtAction(nameof(GetById), new { id = reservation!.Id }, reservation);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var customerId = await GetCustomerIdForCurrentUser();
        if (customerId is null) return NotFound("Customer profile not found.");

        var (success, error) = await _reservationService.CancelForCustomerAsync(id, customerId.Value);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    private async Task<int?> GetCustomerIdForCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var customer = await _customerService.GetByUserIdAsync(userId);
        return customer?.Id;
    }
}
