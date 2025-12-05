using System.Security.Cryptography;
using System.Text;
using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRentalExamen.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationOption> ReservationOptions => Set<ReservationOption>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Return> Returns => Set<Return>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>()
            .HasIndex(c => c.PlateNumber)
            .IsUnique();

        modelBuilder.Entity<Car>()
            .Property(c => c.DailyPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Reservation>()
            .Property(r => r.BasePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Reservation>()
            .Property(r => r.OptionsPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Reservation>()
            .Property(r => r.TotalPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Reservation>()
            .Property(r => r.PickupFuelLevel)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Reservation>()
            .Property(r => r.ReturnFuelLevel)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<ReservationOption>()
            .Property(o => o.PricePerDay)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Return>()
            .Property(r => r.LateFees)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Return>()
            .Property(r => r.DamageFees)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Return>()
            .Property(r => r.FuelFees)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Return>()
            .Property(r => r.TotalExtraFees)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Car)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CarId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CustomerId);

        modelBuilder.Entity<ReservationOption>()
            .HasOne(o => o.Reservation)
            .WithMany(r => r.Options)
            .HasForeignKey(o => o.ReservationId);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Reservation)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.ReservationId);

        modelBuilder.Entity<Return>()
            .HasOne(r => r.Reservation)
            .WithOne(r => r.Return)
            .HasForeignKey<Return>(r => r.ReservationId);

        var admin = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@carrental.com",
            Role = UserRole.Admin,
            PasswordHash = HashPassword("Admin123!")
        };

        var cars = new List<Car>
        {
            new() { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2022, PlateNumber = "ABC-123", DailyPrice = 55, Mileage = 15000, Status = CarStatus.Available, ImageUrl = "" },
            new() { Id = 2, Make = "Tesla", Model = "Model 3", Year = 2023, PlateNumber = "EV-456", DailyPrice = 120, Mileage = 8000, Status = CarStatus.Available, ImageUrl = "" }
        };

        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", CinOrPassport = "AA123456", LicenseNumber = "L-1000", Phone = "555-1000", Email = "john@example.com" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", CinOrPassport = "BB654321", LicenseNumber = "L-2000", Phone = "555-2000", Email = "jane@example.com" }
        };

        modelBuilder.Entity<User>().HasData(admin);
        modelBuilder.Entity<Car>().HasData(cars);
        modelBuilder.Entity<Customer>().HasData(customers);
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
