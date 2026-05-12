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
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null
                    ? await _dbSet.AnyAsync()
                    : await _dbSet.AnyAsync(predicate);
        }
        public async Task<IEnumerable<TResult>> GetAsync<TResult>(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, TResult>>? selector = null, string? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            if (selector != null)
            {
                return await query.Select(selector).ToListAsync();
            }

            return await query.Cast<TResult>().ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            return await query.ToListAsync();
        }
        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, string? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
