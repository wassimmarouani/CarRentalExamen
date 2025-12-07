namespace CarRentalExamen.Core.DTOs.Reservations;

public class ReservationDetailDto
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public string Car { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public decimal BasePrice { get; set; }
    public decimal OptionsPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public IEnumerable<ReservationOptionDto> Options { get; set; } = Array.Empty<ReservationOptionDto>();
    public IEnumerable<PaymentSummaryDto> Payments { get; set; } = Array.Empty<PaymentSummaryDto>();
    public ReturnInfoDto? Return { get; set; }
}

public class PaymentSummaryDto
{
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ReturnInfoDto
{
    public DateTime? ReturnDate { get; set; }
    public decimal LateFees { get; set; }
    public decimal DamageFees { get; set; }
    public decimal FuelFees { get; set; }
    public decimal TotalExtraFees { get; set; }
    public string? Notes { get; set; }
}
