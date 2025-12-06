using System.ComponentModel.DataAnnotations;

namespace CarRentalExamen.Core.DTOs.Payments;

public class PaymentCreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Reservation ID is required")]
    public int ReservationId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(30, ErrorMessage = "Payment method cannot exceed 30 characters")]
    public string Method { get; set; } = "Cash";
}
