using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class OptionRepository : GenericRepository<Option>, IOptionRepository
	{
		public OptionRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}