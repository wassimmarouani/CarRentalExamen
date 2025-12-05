namespace CarRentalExamen.Core.DTOs.Payments;

public class PaymentCreateDto
{
    public int ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
}
