using System.ComponentModel.DataAnnotations;

namespace CarRentalExamen.Core.DTOs.Customers;

public class CustomerCreateUpdateDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "CIN or Passport is required")]
    [StringLength(30, ErrorMessage = "CIN/Passport cannot exceed 30 characters")]
    public string CinOrPassport { get; set; } = string.Empty;

    [Required(ErrorMessage = "License number is required")]
    [StringLength(30, ErrorMessage = "License number cannot exceed 30 characters")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
