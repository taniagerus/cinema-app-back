namespace cinema_app_back.Repositories
{
    using System.Linq.Expressions;

    public interface IGenericRepository<T>
        where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);

        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task AddManyAsync(IEnumerable<T> entities);
        Task DeleteManyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetByPredicateAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task UpdateManyAsync(IEnumerable<T> entities);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }

}
