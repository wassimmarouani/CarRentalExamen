namespace CarRentalExamen.Core.Entities;

public class Return
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal LateFees { get; set; }
    public decimal DamageFees { get; set; }
    public decimal FuelFees { get; set; }
    public string? Notes { get; set; }
    public decimal TotalExtraFees { get; set; }
}
