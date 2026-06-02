using System.Linq.Expressions;

namespace LoopLearn.Entities.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                         string? includes = null,
                                         bool ignoreQueryFilters = false);
        Task<IEnumerable<TResult>> GetAsync<TResult>(Expression<Func<T, bool>>? predicate = null,
                                                     Expression<Func<T, TResult>>? selector = null,
                                                     string? includes = null,
                                                     bool ignoreQueryFilters = false,
                                                     Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null,
                                       string? includes = null,
                                       bool ignoreQueryFilters = false);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<bool> ExistsAsync(Expression<Func<T, bool>>? predicate = null, bool ignoreQueryFilters = false);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
    }
}
