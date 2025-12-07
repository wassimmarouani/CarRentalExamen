using CarRentalExamen.Core.DTOs.Customers;
using CarRentalExamen.Core.Entities;

namespace CarRentalExamen.Core.Interfaces.Services;

/// <summary>
/// Service interface for customer operations
/// </summary>
public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByUserIdAsync(int userId);
    Task<Customer> CreateAsync(CustomerCreateUpdateDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(int id, CustomerCreateUpdateDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
    Task<IEnumerable<Reservation>> GetReservationsAsync(int customerId);
}
