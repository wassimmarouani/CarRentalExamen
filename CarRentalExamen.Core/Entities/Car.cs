using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.Entities;

public class Car
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int Mileage { get; set; }
    public CarStatus Status { get; set; } = CarStatus.Available;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
