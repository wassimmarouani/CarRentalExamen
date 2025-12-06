using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Interfaces.Services;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.API.Controllers;

/// <summary>
/// Returns controller - handles car return operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly AppDbContext _context;

    public ReturnsController(IReservationService reservationService, AppDbContext context)
    {
        _reservationService = reservationService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnRequest request)
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

        var (success, error) = await _reservationService.CompleteAsync(request.ReservationId, serviceRequest);
        if (!success)
        {
            return error == "Reservation not found." ? NotFound(error) : BadRequest(error);
        }

        var returnEntity = await _context.Returns.FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId);
        return Ok(returnEntity);
    }

    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<Return>> Get(int reservationId)
    {
        var returnEntity = await _context.Returns.FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        if (returnEntity is null) return NotFound();
        return Ok(returnEntity);
    }

    public class ReturnRequest
    {
        public int ReservationId { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? ReturnMileage { get; set; }
        public decimal? ReturnFuelLevel { get; set; }
        public decimal? LateFees { get; set; }
        public decimal? DamageFees { get; set; }
        public decimal? FuelFees { get; set; }
        public string? Notes { get; set; }
    }
}
