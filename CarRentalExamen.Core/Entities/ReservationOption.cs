namespace CarRentalExamen.Core.Entities;

public class ReservationOption
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int Quantity { get; set; }
}
