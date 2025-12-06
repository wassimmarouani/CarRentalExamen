using CarRentalExamen.Core.DTOs.Reservations;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces.Services;
using CarRentalExamen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Reservation service - handles all reservation business logic
/// </summary>
public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;

    public ReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReservationDetailDto>> GetAllAsync()
    {
        var reservations = await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Options)
            .Include(r => r.Payments)
            .Include(r => r.Return)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reservations.Select(MapToDetailDto);
    }

    public async Task<ReservationDetailDto?> GetByIdAsync(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Options)
            .Include(r => r.Payments)
            .Include(r => r.Return)
            .FirstOrDefaultAsync(r => r.Id == id);

        return reservation is null ? null : MapToDetailDto(reservation);
    }

    public async Task<(bool Success, string? Error, ReservationDetailDto? Reservation)> CreateAsync(ReservationCreateDto dto)
    {
        var car = await _context.Cars.FindAsync(dto.CarId);
        if (car is null)
        {
            return (false, "Car not found.", null);
        }
        if (car.Status != CarStatus.Available)
        {
            return (false, "Car is not available.", null);
        }

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer is null)
        {
            return (false, "Customer not found.", null);
        }

        if (dto.EndDate <= dto.StartDate)
        {
            return (false, "End date must be after start date.", null);
        }
        if (dto.StartDate.Date < DateTime.UtcNow.Date)
        {
            return (false, "Start date cannot be in the past.", null);
        }

        var hasOverlap = await _context.Reservations.AnyAsync(r =>
            r.CarId == dto.CarId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active) &&
            r.StartDate < dto.EndDate && dto.StartDate < r.EndDate);

        if (hasOverlap)
        {
            return (false, "Car already has an overlapping reservation.", null);
        }

        var totalDays = Math.Max(1, (dto.EndDate.Date - dto.StartDate.Date).Days);
        var basePrice = totalDays * car.DailyPrice;
        var optionsPrice = dto.Options.Sum(o => o.PricePerDay * o.Quantity * totalDays);

        var reservation = new Reservation
        {
            CarId = dto.CarId,
            CustomerId = dto.CustomerId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            BasePrice = basePrice,
            OptionsPrice = optionsPrice,
            TotalPrice = basePrice + optionsPrice,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Options = dto.Options.Select(o => new ReservationOption
            {
                OptionName = o.OptionName,
                PricePerDay = o.PricePerDay,
                Quantity = o.Quantity
            }).ToList()
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Reload with related entities
        reservation = await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Customer)
            .Include(r => r.Options)
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);

        return (true, null, MapToDetailDto(reservation!));
    }

    public async Task<(bool Success, string? Error)> ConfirmAsync(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return (false, "Reservation not found.");
        }
        if (reservation.Status == ReservationStatus.Completed || reservation.Status == ReservationStatus.Cancelled)
        {
            return (false, "Reservation already completed or cancelled.");
        }

        reservation.Status = ReservationStatus.Confirmed;
        if (reservation.Car is not null)
        {
            reservation.Car.Status = CarStatus.Reserved;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return (false, "Reservation not found.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        if (reservation.Car is not null && reservation.Car.Status != CarStatus.Maintenance)
        {
            reservation.Car.Status = CarStatus.Available;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> PickupAsync(int id, int? mileage, decimal? fuelLevel)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return (false, "Reservation not found.");
        }
        if (reservation.Status != ReservationStatus.Confirmed && reservation.Status != ReservationStatus.Pending)
        {
            return (false, "Reservation must be confirmed before pickup.");
        }

        reservation.Status = ReservationStatus.Active;
        reservation.PickedUpAt = DateTime.UtcNow;
        reservation.PickupMileage = mileage;
        reservation.PickupFuelLevel = fuelLevel;

        if (reservation.Car is not null)
        {
            reservation.Car.Status = CarStatus.Rented;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(int id, CompleteReservationRequest request)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .Include(r => r.Return)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
        {
            return (false, "Reservation not found.");
        }
        if (reservation.Status == ReservationStatus.Completed)
        {
            return (false, "Reservation is already completed.");
        }
        if (reservation.Status == ReservationStatus.Cancelled)
        {
            return (false, "Cannot complete a cancelled reservation.");
        }

        // Validate fees
        if (request.LateFees.HasValue && request.LateFees.Value < 0)
        {
            return (false, "Late fees cannot be negative.");
        }
        if (request.DamageFees.HasValue && request.DamageFees.Value < 0)
        {
            return (false, "Damage fees cannot be negative.");
        }
        if (request.FuelFees.HasValue && request.FuelFees.Value < 0)
        {
            return (false, "Fuel fees cannot be negative.");
        }

        reservation.Status = ReservationStatus.Completed;
        reservation.ReturnedAt = request.ReturnDate ?? DateTime.UtcNow;
        reservation.ReturnMileage = request.ReturnMileage;
        reservation.ReturnFuelLevel = request.ReturnFuelLevel;

        if (reservation.Car is not null)
        {
            reservation.Car.Status = CarStatus.Available;
            if (request.ReturnMileage.HasValue)
            {
                reservation.Car.Mileage = request.ReturnMileage.Value;
            }
        }

        var lateFees = request.LateFees ?? CalculateLateFee(reservation);
        var fuelFees = request.FuelFees ?? CalculateFuelFee(reservation);
        var damageFees = request.DamageFees ?? 0;
        var totalExtra = lateFees + fuelFees + damageFees;

        if (reservation.Return is null)
        {
            reservation.Return = new Return
            {
                ReservationId = reservation.Id,
                ReturnDate = request.ReturnDate ?? DateTime.UtcNow,
                LateFees = lateFees,
                DamageFees = damageFees,
                FuelFees = fuelFees,
                Notes = request.Notes,
                TotalExtraFees = totalExtra
            };
        }
        else
        {
            reservation.Return.ReturnDate = request.ReturnDate ?? DateTime.UtcNow;
            reservation.Return.LateFees = lateFees;
            reservation.Return.DamageFees = damageFees;
            reservation.Return.FuelFees = fuelFees;
            reservation.Return.TotalExtraFees = totalExtra;
            reservation.Return.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static ReservationDetailDto MapToDetailDto(Reservation reservation)
    {
        return new ReservationDetailDto
        {
            Id = reservation.Id,
            Car = reservation.Car is null ? $"Car {reservation.CarId}" : $"{reservation.Car.Make} {reservation.Car.Model}",
            Customer = reservation.Customer is null ? $"Customer {reservation.CustomerId}" : $"{reservation.Customer.FirstName} {reservation.Customer.LastName}",
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            TotalDays = reservation.TotalDays,
            BasePrice = reservation.BasePrice,
            OptionsPrice = reservation.OptionsPrice,
            TotalPrice = reservation.TotalPrice,
            Status = reservation.Status.ToString(),
            Options = reservation.Options.Select(o => new ReservationOptionDto
            {
                OptionName = o.OptionName,
                PricePerDay = o.PricePerDay,
                Quantity = o.Quantity
            }),
            Payments = reservation.Payments.Select(p => new PaymentSummaryDto
            {
                Amount = p.Amount,
                PaidAt = p.PaidAt,
                Method = p.Method,
                Status = p.Status.ToString()
            }),
            Return = reservation.Return is null ? null : new ReturnInfoDto
            {
                ReturnDate = reservation.Return.ReturnDate,
                LateFees = reservation.Return.LateFees,
                DamageFees = reservation.Return.DamageFees,
                FuelFees = reservation.Return.FuelFees,
                TotalExtraFees = reservation.Return.TotalExtraFees,
                Notes = reservation.Return.Notes
            }
        };
    }

    private static decimal CalculateLateFee(Reservation reservation)
    {
        if (reservation.ReturnedAt.HasValue && reservation.ReturnedAt.Value.Date > reservation.EndDate.Date)
        {
            var daysLate = (reservation.ReturnedAt.Value.Date - reservation.EndDate.Date).Days;
            return daysLate * 20m;
        }
        return 0;
    }

    private static decimal CalculateFuelFee(Reservation reservation)
    {
        if (reservation.PickupFuelLevel.HasValue && reservation.ReturnFuelLevel.HasValue)
        {
            var diff = reservation.PickupFuelLevel.Value - reservation.ReturnFuelLevel.Value;
            return diff > 0 ? diff * 30m : 0;
        }
        return 0;
    }
}
