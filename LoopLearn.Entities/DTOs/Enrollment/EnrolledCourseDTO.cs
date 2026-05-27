using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopLearn.Entities.DTOs.Enrollment
{
	public class EnrolledCourseDTO
	{
		public int CourseId { get; set; }
		public string Title { get; set; }
		public string Subtitle { get; set; }
		public string ThumbnailUrl { get; set; }
		public string InstructorName { get; set; }
		public double ProgressPercentage { get; set; }
		public bool IsCourseAvailable { get; set; }
		public bool IsCompleted { get; set; }
		public DateTime EnrolledAt { get; set; }
		public DateTime? LastAccessAt { get; set; }
		public DateTime? CompletedAt { get; set; }
	}
}
