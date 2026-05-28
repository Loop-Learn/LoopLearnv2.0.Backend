using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
    public class CourseReviewHistoryRepository : GenericRepository<CourseReviewHistory>, ICourseReviewHistoryRepository
    {
        public CourseReviewHistoryRepository(ApplicationDbContext context) : base(context) { }
    }
}
