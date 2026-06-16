using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class InstructorApplicationRepository : GenericRepository<InstructorApplication>, IInstructorApplicationRepository
    {
        private readonly ApplicationDbContext _context;
        public InstructorApplicationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}