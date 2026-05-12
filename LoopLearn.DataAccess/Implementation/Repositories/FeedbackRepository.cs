using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        private readonly ApplicationDbContext _context;
        public FeedbackRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
