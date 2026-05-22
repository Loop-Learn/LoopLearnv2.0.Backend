using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        private readonly ApplicationDbContext _context;
        public TagRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
