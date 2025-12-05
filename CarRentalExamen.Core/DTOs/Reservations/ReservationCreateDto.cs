namespace CarRentalExamen.Core.DTOs.Reservations;

public class ReservationCreateDto
{
    public int CarId { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ReservationOptionDto> Options { get; set; } = new();
}
