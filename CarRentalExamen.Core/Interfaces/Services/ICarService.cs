using CarRentalExamen.Core.DTOs.Cars;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.Interfaces.Services;

/// <summary>
/// Service interface for car operations
/// </summary>
public interface ICarService
{
    Task<IEnumerable<Car>> GetAllAsync(CarStatus? status = null);
    Task<Car?> GetByIdAsync(int id);
    Task<Car?> GetByIdWithReservationsAsync(int id);
    Task<(bool Success, string? Error, Car? Car)> CreateAsync(CarCreateUpdateDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(int id, CarCreateUpdateDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int id, CarStatus status);
    Task<bool> PlateNumberExistsAsync(string plateNumber, int? excludeId = null);
}
