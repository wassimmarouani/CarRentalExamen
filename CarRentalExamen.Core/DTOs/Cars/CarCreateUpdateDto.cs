using System.ComponentModel.DataAnnotations;
using CarRentalExamen.Core.Enums;

namespace CarRentalExamen.Core.DTOs.Cars;

public class CarCreateUpdateDto
{
    [Required(ErrorMessage = "Make is required")]
    [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters")]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Model is required")]
    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Plate number is required")]
    [StringLength(20, ErrorMessage = "Plate number cannot exceed 20 characters")]
    public string PlateNumber { get; set; } = string.Empty;

    [Range(0.01, 10000, ErrorMessage = "Daily price must be between 0.01 and 10000")]
    public decimal DailyPrice { get; set; }

    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative")]
    public int Mileage { get; set; }

    public CarStatus Status { get; set; } = CarStatus.Available;
}
