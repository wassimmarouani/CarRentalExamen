using CarRentalExamen.Core.DTOs.Payments;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Payment service - handles payment creation and retrieval
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string? Error, Payment? Payment)> CreateAsync(PaymentCreateDto dto)
    {
        if (dto.Amount <= 0)
        {
            return (false, "Payment amount must be greater than zero.", null);
        }

        var reservation = await _unitOfWork.Reservations.Query()
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

        await _unitOfWork.Payments.AddAsync(payment);

        // Calculate payment status
        var totalPaid = reservation.Payments.Sum(p => p.Amount) + dto.Amount;
        var totalDue = reservation.TotalPrice + (reservation.Return?.TotalExtraFees ?? 0);
        var status = totalPaid >= totalDue ? PaymentStatus.Paid : PaymentStatus.Partial;

        payment.Status = status;
        foreach (var existing in reservation.Payments)
        {
            existing.Status = status;
        }

        await _unitOfWork.SaveChangesAsync();
        return (true, null, payment);
    }

    public async Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId)
    {
        return await _unitOfWork.Payments.Query()
            .Where(p => p.ReservationId == reservationId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }
}
