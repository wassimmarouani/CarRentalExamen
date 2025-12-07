using CarRentalExamen.Core.DTOs.Reservations;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalExamen.API.Controllers;

/// <summary>
/// Reservations controller - thin controller that delegates to ReservationService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDetailDto>>> GetAll()
    {
        var reservations = await _reservationService.GetAllAsync();
        return Ok(reservations);
    }

    [HttpPost("quote")]
    [AllowAnonymous]
    public async Task<ActionResult<ReservationQuoteResponseDto>> Quote([FromBody] ReservationQuoteRequestDto request)
    {
        var quote = await _reservationService.QuoteAsync(request);
        if (!quote.IsAvailable)
        {
            return Conflict(quote);
        }
        return Ok(quote);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationDetailDto>> GetById(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation is null) return NotFound();
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDetailDto>> Create([FromBody] ReservationCreateDto dto)
    {
        var (success, error, reservation) = await _reservationService.CreateAsync(dto);
        if (!success)
        {
            return error!.Contains("overlap") ? Conflict(error) : BadRequest(error);
        }
        return CreatedAtAction(nameof(GetById), new { id = reservation!.Id }, reservation);
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var (success, error) = await _reservationService.ConfirmAsync(id);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _reservationService.CancelAsync(id);
        if (!success)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    [HttpPut("{id:int}/pickup")]
    public async Task<IActionResult> Pickup(int id, [FromBody] PickupRequest request)
    {
        var (success, error) = await _reservationService.PickupAsync(id, request.PickupMileage, request.PickupFuelLevel);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteRequest request)
    {
        var serviceRequest = new CompleteReservationRequest
        {
            ReturnDate = request.ReturnDate,
            ReturnMileage = request.ReturnMileage,
            ReturnFuelLevel = request.ReturnFuelLevel,
            LateFees = request.LateFees,
            DamageFees = request.DamageFees,
            FuelFees = request.FuelFees,
            Notes = request.Notes
        };

        var (success, error) = await _reservationService.CompleteAsync(id, serviceRequest);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound() : BadRequest(error);
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _reservationService.DeleteAsync(id);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound(error) : BadRequest(error);
        }
        return NoContent();
    }

    // Request DTOs for this controller
    public class PickupRequest
    {
        public int? PickupMileage { get; set; }
        public decimal? PickupFuelLevel { get; set; }
    }

    public class CompleteRequest
    {
        public DateTime? ReturnDate { get; set; }
        public int? ReturnMileage { get; set; }
        public decimal? ReturnFuelLevel { get; set; }
        public decimal? LateFees { get; set; }
        public decimal? DamageFees { get; set; }
        public decimal? FuelFees { get; set; }
        public string? Notes { get; set; }
    }
}
