using CarRentalExamen.Core.DTOs.Payments;
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
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> Create([FromBody] PaymentCreateDto dto)
    {
        if (dto.Amount <= 0)
        {
            return BadRequest("Payment amount must be greater than zero.");
        }

        var reservation = await _context.Reservations
            .Include(r => r.Payments)
            .Include(r => r.Return)
            .FirstOrDefaultAsync(r => r.Id == dto.ReservationId);

        if (reservation is null) return NotFound("Reservation not found");

        var payment = new Payment
        {
            ReservationId = dto.ReservationId,
            Amount = dto.Amount,
            PaidAt = DateTime.UtcNow,
            Method = dto.Method
        };

        _context.Payments.Add(payment);

        var totalPaid = reservation.Payments.Sum(p => p.Amount) + dto.Amount;
        var totalDue = reservation.TotalPrice + (reservation.Return?.TotalExtraFees ?? 0);
        var status = totalPaid >= totalDue ? PaymentStatus.Paid : PaymentStatus.Partial;

        payment.Status = status;
        foreach (var existing in reservation.Payments)
        {
            existing.Status = status;
        }

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByReservation), new { reservationId = reservation.Id }, payment);
    }

    [HttpGet("{reservationId:int}")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetByReservation(int reservationId)
    {
        var payments = await _context.Payments
            .Where(p => p.ReservationId == reservationId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        return Ok(payments);
    }
}
