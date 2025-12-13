using CarRentalExamen.Core.Enums;
using CarRentalExamen.Core.Interfaces;
using CarRentalExamen.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Dashboard service - provides statistics and analytics
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var revenue = await _unitOfWork.Payments.Query().SumAsync(p => (decimal?)p.Amount) ?? 0;
        var activeRentals = await _unitOfWork.Reservations.Query().CountAsync(r => r.Status == ReservationStatus.Active);
        var availableCars = await _unitOfWork.Cars.Query().CountAsync(c => c.Status == CarStatus.Available);

        var rentalsPerMonth = await _unitOfWork.Reservations.Query()
            .GroupBy(r => new { r.StartDate.Year, r.StartDate.Month })
            .Select(g => new RentalMonthStatDto 
            { 
                Year = g.Key.Year, 
                Month = g.Key.Month, 
                Count = g.Count() 
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync();

        var topCars = await _unitOfWork.Reservations.Query()
            .Include(r => r.Car)
            .GroupBy(r => r.CarId)
            .Select(g => new TopCarStatDto
            {
                CarId = g.Key,
                Car = g.First().Car != null ? $"{g.First().Car!.Make} {g.First().Car!.Model}" : $"Car {g.Key}",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return new DashboardStatsDto
        {
            Revenue = revenue,
            ActiveRentals = activeRentals,
            AvailableCars = availableCars,
            RentalsPerMonth = rentalsPerMonth,
            TopCars = topCars
        };
    }
}
