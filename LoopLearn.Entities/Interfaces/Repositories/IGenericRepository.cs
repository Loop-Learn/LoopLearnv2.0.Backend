using System.Linq.Expressions;

namespace LoopLearn.Entities.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string? includes = null);
        Task<IEnumerable<TResult>> GetAsync<TResult>(Expression<Func<T, bool>>? predicate = null,
                                                     Expression<Func<T, TResult>>? selector = null,
                                                     string? include = null);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, string? includes = null);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<bool> ExistsAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
