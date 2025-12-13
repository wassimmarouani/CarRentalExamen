using CarRentalExamen.Core.DTOs.Customers;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Customer service - handles all customer-related business logic
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _unitOfWork.Customers.Query().AsNoTracking().ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Customers.GetByIdAsync(id);
    }

    public async Task<Customer?> GetByUserIdAsync(int userId)
    {
        return await _unitOfWork.Customers.Query().FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Customer> CreateAsync(CustomerCreateUpdateDto dto)
    {
        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CinOrPassport = dto.CinOrPassport,
            LicenseNumber = dto.LicenseNumber,
            Phone = dto.Phone,
            Email = dto.Email
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();
        return customer;
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, CustomerCreateUpdateDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer is null)
        {
            return (false, "Customer not found.");
        }

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.CinOrPassport = dto.CinOrPassport;
        customer.LicenseNumber = dto.LicenseNumber;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var customer = await _unitOfWork.Customers.Query()
            .Include(c => c.Reservations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return (false, "Customer not found.");
        }

        var hasActiveReservations = customer.Reservations.Any(r =>
            r.Status == ReservationStatus.Pending ||
            r.Status == ReservationStatus.Confirmed ||
            r.Status == ReservationStatus.Active);

        if (hasActiveReservations)
        {
            return (false, "Cannot delete customer with active reservations.");
        }

        _unitOfWork.Customers.Delete(customer);
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(int customerId)
    {
        return await _unitOfWork.Reservations.Query()
            .Include(r => r.Car)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
