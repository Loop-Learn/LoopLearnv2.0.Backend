using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class QuizAttemptRepository : GenericRepository<QuizAttempt>, IQuizAttemptRepository
    {
        private readonly ApplicationDbContext _context;
        public QuizAttemptRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
