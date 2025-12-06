using CarRentalExamen.Core.DTOs.Payments;
using CarRentalExamen.Core.Entities;

namespace CarRentalExamen.Core.Interfaces.Services;

/// <summary>
/// Service interface for payment operations
/// </summary>
public interface IPaymentService
{
    Task<(bool Success, string? Error, Payment? Payment)> CreateAsync(PaymentCreateDto dto);
    Task<IEnumerable<Payment>> GetByReservationAsync(int reservationId);
}
