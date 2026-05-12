using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
