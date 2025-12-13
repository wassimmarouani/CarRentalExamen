using System.Linq.Expressions;

namespace CarRentalExamen.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
}
