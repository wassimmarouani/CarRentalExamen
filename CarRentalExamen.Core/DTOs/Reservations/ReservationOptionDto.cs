using System.ComponentModel.DataAnnotations;

namespace CarRentalExamen.Core.DTOs.Reservations;

public class ReservationOptionDto
{
    [Required(ErrorMessage = "Option name is required")]
    [StringLength(100, ErrorMessage = "Option name cannot exceed 100 characters")]
    public string OptionName { get; set; } = string.Empty;

    [Range(0, 1000, ErrorMessage = "Price per day must be between 0 and 1000")]
    public decimal PricePerDay { get; set; }

    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}
