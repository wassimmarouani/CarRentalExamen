using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car? Car { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public decimal BasePrice { get; set; }
    public decimal OptionsPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int? PickupMileage { get; set; }
    public decimal? PickupFuelLevel { get; set; }
    public int? ReturnMileage { get; set; }
    public decimal? ReturnFuelLevel { get; set; }

    public Return? Return { get; set; }
    public ICollection<ReservationOption> Options { get; set; } = new List<ReservationOption>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
