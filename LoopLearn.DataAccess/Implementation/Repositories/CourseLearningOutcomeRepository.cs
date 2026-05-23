using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class CourseLearningOutcomeRepository : GenericRepository<CourseLearningOutcome>, ICourseLearningOutcomeRepository
	{
		public CourseLearningOutcomeRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}