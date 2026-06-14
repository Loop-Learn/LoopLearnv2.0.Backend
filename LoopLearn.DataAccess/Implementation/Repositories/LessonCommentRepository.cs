using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class LessonCommentRepository : GenericRepository<LessonComment> , ILessonCommentRepository
    {
        private readonly ApplicationDbContext _context;

        public LessonCommentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
