namespace CarRentalExamen.Core.DTOs.Reservations;

public class ReservationOptionDto
{
    public string OptionName { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int Quantity { get; set; }
}
