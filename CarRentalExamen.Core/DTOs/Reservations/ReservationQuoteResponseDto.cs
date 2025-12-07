namespace CarRentalExamen.Core.DTOs.Reservations;

public class ReservationQuoteResponseDto
{
    public int TotalDays { get; set; }
    public decimal BasePrice { get; set; }
    public decimal OptionsPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsAvailable { get; set; }
    public string? Error { get; set; }
}
