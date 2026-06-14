using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class StudentAnswersRepository : GenericRepository<StudentAnswer>, IStudentAnswersRepository
    {
        private readonly ApplicationDbContext _context;
        public StudentAnswersRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
