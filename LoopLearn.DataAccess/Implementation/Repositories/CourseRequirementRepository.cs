using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class CourseRequirementRepository : GenericRepository<CourseRequirement>, ICourseRequirementRepository
	{
		public CourseRequirementRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}