using LoopLearn.DataAccess.Data;
using LoopLearn.Entities.Interfaces.Repositories;
using LoopLearn.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopLearn.DataAccess.Implementation.Repositories
{
	public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
	{
		private readonly ApplicationDbContext _context;
		public PaymentRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
