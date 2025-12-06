using CarRentalExamen.Core.DTOs.Cars;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces.Services;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Car service - handles all car-related business logic
/// </summary>
public class CarService : ICarService
{
    private readonly AppDbContext _context;

    public CarService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Car>> GetAllAsync(CarStatus? status = null)
    {
        var query = _context.Cars.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status);
        }
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        return await _context.Cars.FindAsync(id);
    }

    public async Task<Car?> GetByIdWithReservationsAsync(int id)
    {
        return await _context.Cars
            .Include(c => c.Reservations)
            .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(bool Success, string? Error, Car? Car)> CreateAsync(CarCreateUpdateDto dto)
    {
        if (await PlateNumberExistsAsync(dto.PlateNumber))
        {
            return (false, "Plate number already exists.", null);
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
        return (true, null, car);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, CarCreateUpdateDto dto)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car is null)
        {
            return (false, "Car not found.");
        }

        if (await PlateNumberExistsAsync(dto.PlateNumber, id))
        {
            return (false, "Plate number already exists.");
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
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var car = await _context.Cars
            .Include(c => c.Reservations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car is null)
        {
            return (false, "Car not found.");
        }

        var hasActiveReservation = car.Reservations.Any(r =>
            r.Status == ReservationStatus.Pending ||
            r.Status == ReservationStatus.Confirmed ||
            r.Status == ReservationStatus.Active);

        if (hasActiveReservation)
        {
            return (false, "Cannot delete car with active reservations.");
        }

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, CarStatus status)
    {
        var car = await _context.Cars
            .Include(c => c.Reservations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car is null)
        {
            return (false, "Car not found.");
        }

        var hasActiveReservation = car.Reservations.Any(r =>
            r.Status == ReservationStatus.Active ||
            r.Status == ReservationStatus.Confirmed);

        if (status == CarStatus.Available && hasActiveReservation)
        {
            return (false, "Cannot set car to Available while it has active or confirmed reservations.");
        }

        car.Status = status;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> PlateNumberExistsAsync(string plateNumber, int? excludeId = null)
    {
        var query = _context.Cars.Where(c => c.PlateNumber == plateNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }
}
