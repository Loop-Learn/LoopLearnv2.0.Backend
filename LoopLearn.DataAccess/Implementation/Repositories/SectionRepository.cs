using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class SectionRepository : GenericRepository<Section>, ISectionRepository
	{
		public SectionRepository(ApplicationDbContext context) : base(context)
		{
		}
	}
}