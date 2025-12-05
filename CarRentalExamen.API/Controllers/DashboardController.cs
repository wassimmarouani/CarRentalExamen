using CarRentalExamen.Core.Enums;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var revenue = await _context.Payments.SumAsync(p => (decimal?)p.Amount) ?? 0;
        var activeRentals = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Active);
        var availableCars = await _context.Cars.CountAsync(c => c.Status == CarStatus.Available);

        var rentalsPerMonth = await _context.Reservations
            .GroupBy(r => new { r.StartDate.Year, r.StartDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        var topCars = await _context.Reservations
            .Include(r => r.Car)
            .GroupBy(r => r.CarId)
            .Select(g => new
            {
                CarId = g.Key,
                Car = g.First().Car != null ? $"{g.First().Car.Make} {g.First().Car.Model}" : $"Car {g.Key}",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            revenue,
            activeRentals,
            availableCars,
            rentalsPerMonth,
            topCars
        });
    }
}
