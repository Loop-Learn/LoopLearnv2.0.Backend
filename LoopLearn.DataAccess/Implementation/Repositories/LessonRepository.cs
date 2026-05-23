using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class LessonRepository : GenericRepository<Lesson>, ILessonRepository
	{
		public LessonRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}