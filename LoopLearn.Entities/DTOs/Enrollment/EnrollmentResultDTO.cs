using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopLearn.Entities.DTOs.Enrollment
{
	public class EnrollmentResultDTO
	{
		public int CourseId { get; set; }
		public string CourseTitle { get; set; }
		public DateTime EnrolledAt { get; set; }
		public bool IsFree { get; set; }
	}
}
