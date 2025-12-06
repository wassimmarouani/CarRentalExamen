using CarRentalExamen.Core.DTOs.Reservations;
using CarRentalExamen.Core.Entities;

namespace CarRentalExamen.Core.Interfaces.Services;

/// <summary>
/// Service interface for reservation operations
/// </summary>
public interface IReservationService
{
    Task<IEnumerable<ReservationDetailDto>> GetAllAsync();
    Task<ReservationDetailDto?> GetByIdAsync(int id);
    Task<(bool Success, string? Error, ReservationDetailDto? Reservation)> CreateAsync(ReservationCreateDto dto);
    Task<(bool Success, string? Error)> ConfirmAsync(int id);
    Task<(bool Success, string? Error)> CancelAsync(int id);
    Task<(bool Success, string? Error)> PickupAsync(int id, int? mileage, decimal? fuelLevel);
    Task<(bool Success, string? Error)> CompleteAsync(int id, CompleteReservationRequest request);
}

public class CompleteReservationRequest
{
    public DateTime? ReturnDate { get; set; }
    public int? ReturnMileage { get; set; }
    public decimal? ReturnFuelLevel { get; set; }
    public decimal? LateFees { get; set; }
    public decimal? DamageFees { get; set; }
    public decimal? FuelFees { get; set; }
    public string? Notes { get; set; }
}
