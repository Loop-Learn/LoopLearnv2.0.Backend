using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>>? predicate = null,
            bool ignoreQueryFilters = false)
        {
            // BUG FIX: was falling back to _dbSet.AnyAsync() when ignoreQueryFilters=true,
            // completely discarding the IgnoreQueryFilters() call on `query`.
            // Now everything goes through `query` consistently.
            IQueryable<T> query = _dbSet;

            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            return predicate == null
                ? await query.AnyAsync()
                : await query.AnyAsync(predicate);
        }

        /// <summary>
        /// Projects and filters entities at the DB level.
        /// </summary>
        /// <param name="orderBy">
        ///     Optional sort to apply before materializing — runs at the IQueryable level,
        ///     so no in-memory sort after loading all rows.
        ///     Example: <c>q => q.OrderByDescending(p => p.CreatedAt)</c>
        /// </param>
        public async Task<IEnumerable<TResult>> GetAsync<TResult>(
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, TResult>>? selector = null,
            string? includes = null,
            bool ignoreQueryFilters = false,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (predicate != null)
                query = query.Where(predicate);

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(include.Trim());
            }

            if (orderBy != null)
                query = orderBy(query);

            if (selector != null)
                return await query.Select(selector).ToListAsync();

            return await query.Cast<TResult>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includes = null,
            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;

            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (predicate != null)
                query = query.Where(predicate);

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(include.Trim());
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includes = null,
            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;

            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (predicate != null)
                query = query.Where(predicate);

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(include.Trim());
            }

            return await query.FirstOrDefaultAsync();
        }

        public void Remove(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return;

            _dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
        }
    }
}