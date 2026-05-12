using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        public CourseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
