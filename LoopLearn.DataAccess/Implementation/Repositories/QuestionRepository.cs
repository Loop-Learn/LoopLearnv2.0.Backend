using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
	{
		public QuestionRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}