using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.Entities;

public class Payment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string Method { get; set; } = "Cash";
    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
}
