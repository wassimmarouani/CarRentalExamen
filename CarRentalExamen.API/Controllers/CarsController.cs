using CarRentalExamen.Core.DTOs.Cars;
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
public class CarsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Car>>> GetAll([FromQuery] CarStatus? status)
    {
        var query = _context.Cars.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status);
        }

        var cars = await query.AsNoTracking().ToListAsync();
        return Ok(cars);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Car>> GetById(int id)
    {
        var car = await _context.Cars
            .Include(c => c.Reservations)
            .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car is null) return NotFound();
        return Ok(car);
    }

    [HttpPost]
    public async Task<ActionResult<Car>> Create([FromBody] CarCreateUpdateDto dto)
    {
        if (await _context.Cars.AnyAsync(c => c.PlateNumber == dto.PlateNumber))
        {
            return Conflict("Plate number already exists.");
        }

        var car = new Car
        {
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            PlateNumber = dto.PlateNumber,
            DailyPrice = dto.DailyPrice,
            ImageUrl = dto.ImageUrl,
            Mileage = dto.Mileage,
            Status = dto.Status
        };
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CarCreateUpdateDto dto)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car is null) return NotFound();

        if (car.PlateNumber != dto.PlateNumber &&
            await _context.Cars.AnyAsync(c => c.PlateNumber == dto.PlateNumber))
        {
            return Conflict("Plate number already exists.");
        }

        car.Make = dto.Make;
        car.Model = dto.Model;
        car.Year = dto.Year;
        car.PlateNumber = dto.PlateNumber;
        car.DailyPrice = dto.DailyPrice;
        car.ImageUrl = dto.ImageUrl;
        car.Mileage = dto.Mileage;
        car.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _context.Cars.Include(c => c.Reservations).FirstOrDefaultAsync(c => c.Id == id);
        if (car is null) return NotFound();

        var hasActiveReservation = car.Reservations.Any(r =>
            r.Status == ReservationStatus.Pending ||
            r.Status == ReservationStatus.Confirmed ||
            r.Status == ReservationStatus.Active);
        if (hasActiveReservation)
        {
            return BadRequest("Cannot delete car with active reservations.");
        }

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] CarStatus status)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car is null) return NotFound();
        car.Status = status;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
