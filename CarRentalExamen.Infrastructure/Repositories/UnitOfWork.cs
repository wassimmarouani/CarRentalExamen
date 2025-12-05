using CarRentalExamen.Core.Entities;
using CarRentalExamen.Core.Interfaces;
using CarRentalExamen.Infrastructure.Data;

namespace CarRentalExamen.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(_context);
        Cars = new GenericRepository<Car>(_context);
        Customers = new GenericRepository<Customer>(_context);
        Reservations = new GenericRepository<Reservation>(_context);
        ReservationOptions = new GenericRepository<ReservationOption>(_context);
        Payments = new GenericRepository<Payment>(_context);
        Returns = new GenericRepository<Return>(_context);
    }

    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Car> Cars { get; }
    public IGenericRepository<Customer> Customers { get; }
    public IGenericRepository<Reservation> Reservations { get; }
    public IGenericRepository<ReservationOption> ReservationOptions { get; }
    public IGenericRepository<Payment> Payments { get; }
    public IGenericRepository<Return> Returns { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
