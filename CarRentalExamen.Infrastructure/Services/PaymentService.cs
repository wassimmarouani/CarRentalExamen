using CarRentalExamen.Core.DTOs.Payments;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces.Services;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Payment service - handles payment creation and retrieval
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? Error, Payment? Payment)> CreateAsync(PaymentCreateDto dto)
    {
        if (dto.Amount <= 0)
        {
            return (false, "Payment amount must be greater than zero.", null);
        }

        var reservation = await _context.Reservations
            .Include(r => r.Payments)
            .Include(r => r.Return)
            .FirstOrDefaultAsync(r => r.Id == dto.ReservationId);

        if (reservation is null)
        {
            return (false, "Reservation not found.", null);
        }

        var payment = new Payment
        {
            ReservationId = dto.ReservationId,
            Amount = dto.Amount,
            PaidAt = DateTime.UtcNow,
            Method = dto.Method
        };

        _context.Payments.Add(payment);

        // Calculate payment status
        var totalPaid = reservation.Payments.Sum(p => p.Amount) + dto.Amount;
        var totalDue = reservation.TotalPrice + (reservation.Return?.TotalExtraFees ?? 0);
        var status = totalPaid >= totalDue ? PaymentStatus.Paid : PaymentStatus.Partial;

        payment.Status = status;
        foreach (var existing in reservation.Payments)
        {
            existing.Status = status;
        }

        await _context.SaveChangesAsync();
        return (true, null, payment);
    }

    public async Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId)
    {
        return await _context.Payments
            .Where(p => p.ReservationId == reservationId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }
}
