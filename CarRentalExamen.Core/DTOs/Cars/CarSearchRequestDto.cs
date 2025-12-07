using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.DTOs.Cars;

public class CarSearchRequestDto
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public decimal? MinDailyPrice { get; set; }
    public decimal? MaxDailyPrice { get; set; }
    public int? MaxMileage { get; set; }
    public CarStatus? Status { get; set; } = CarStatus.Available;
}
