using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class LessonProgressRepository : GenericRepository<StudentLessonProgress>, ILessonProgressRepository
    {
        private readonly ApplicationDbContext _context;
        public LessonProgressRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
