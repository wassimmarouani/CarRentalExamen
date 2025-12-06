using System.ComponentModel.DataAnnotations;

namespace CarRentalExamen.Core.DTOs.Reservations;

public class ReservationCreateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Car ID is required")]
    public int CarId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public List<ReservationOptionDto> Options { get; set; } = new();
}
