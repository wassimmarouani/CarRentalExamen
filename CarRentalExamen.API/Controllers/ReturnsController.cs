using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReturnsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnRequest request)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Return)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId);

        if (reservation is null) return NotFound("Reservation not found");

        var returnDate = request.ReturnDate ?? DateTime.UtcNow;
        var daysLate = returnDate.Date > reservation.EndDate.Date ? (returnDate.Date - reservation.EndDate.Date).Days : 0;
        var lateFees = request.LateFees ?? (daysLate * 25m);
        var fuelFees = request.FuelFees ?? CalculateFuelFee(reservation.PickupFuelLevel, request.ReturnFuelLevel);
        var damageFees = request.DamageFees ?? 0;
        var totalExtra = lateFees + fuelFees + damageFees;

        var returnEntity = reservation.Return ?? new Return { ReservationId = reservation.Id };
        returnEntity.ReturnDate = returnDate;
        returnEntity.LateFees = lateFees;
        returnEntity.DamageFees = damageFees;
        returnEntity.FuelFees = fuelFees;
        returnEntity.TotalExtraFees = totalExtra;
        returnEntity.Notes = request.Notes;

        reservation.Return = returnEntity;
        reservation.Status = ReservationStatus.Completed;
        reservation.ReturnedAt = returnDate;
        reservation.ReturnMileage = request.ReturnMileage;
        reservation.ReturnFuelLevel = request.ReturnFuelLevel;

        if (reservation.Car is not null)
        {
            reservation.Car.Status = CarStatus.Available;
            if (request.ReturnMileage.HasValue)
            {
                reservation.Car.Mileage = request.ReturnMileage.Value;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(returnEntity);
    }

    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<Return>> Get(int reservationId)
    {
        var returnEntity = await _context.Returns.FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        if (returnEntity is null) return NotFound();
        return Ok(returnEntity);
    }

    private static decimal CalculateFuelFee(decimal? pickupFuel, decimal? returnFuel)
    {
        if (pickupFuel.HasValue && returnFuel.HasValue)
        {
            var diff = pickupFuel.Value - returnFuel.Value;
            return diff > 0 ? diff * 30m : 0;
        }
        return 0;
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
