using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class CourseTargetAudienceRepository : GenericRepository<CourseTargetAudience>, ICourseTargetAudienceRepository
	{
		public CourseTargetAudienceRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}