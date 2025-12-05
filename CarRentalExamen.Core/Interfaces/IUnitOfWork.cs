using CarRentalExamen.Core.Entities;

namespace CarRentalExamen.Core.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Car> Cars { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<Reservation> Reservations { get; }
    IGenericRepository<ReservationOption> ReservationOptions { get; }
    IGenericRepository<Payment> Payments { get; }
    IGenericRepository<Return> Returns { get; }
    Task<int> SaveChangesAsync();
}
