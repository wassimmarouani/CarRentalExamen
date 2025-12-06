namespace CarRentalExamen.Core.Interfaces.Services;

/// <summary>
/// Service interface for dashboard statistics
/// </summary>
public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}

public class DashboardStatsDto
{
    public decimal Revenue { get; set; }
    public int ActiveRentals { get; set; }
    public int AvailableCars { get; set; }
    public IEnumerable<RentalMonthStatDto> RentalsPerMonth { get; set; } = Array.Empty<RentalMonthStatDto>();
    public IEnumerable<TopCarStatDto> TopCars { get; set; } = Array.Empty<TopCarStatDto>();
}

public class RentalMonthStatDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public class TopCarStatDto
{
    public int CarId { get; set; }
    public string Car { get; set; } = string.Empty;
    public int Count { get; set; }
}
