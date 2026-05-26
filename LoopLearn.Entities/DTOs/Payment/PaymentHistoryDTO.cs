using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopLearn.Entities.DTOs.Payment
{
	public class PaymentHistoryDTO
	{
		public int Id { get; set; }
		public int CourseId { get; set; }
		public string CourseTitle { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; }

	}
}
