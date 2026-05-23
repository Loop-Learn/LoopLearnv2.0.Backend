using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class CourseTagRepository : GenericRepository<CourseTag>, ICourseTagRepository
	{
		public CourseTagRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}