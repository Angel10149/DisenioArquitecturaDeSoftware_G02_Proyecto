using System.Linq.Expressions;

namespace AppWeb.Repositories
{
    /// <summary>
    /// Contrato genérico de acceso a datos para entidades del dominio.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveChangesAsync();
    }
}
