
namespace LoopLearn.Entities.DTOs.Course
{
	public class InstructorStudentDTO
	{
		public string StudentId { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string ProfileImageUrl { get; set; }

		public List<StudentEnrolledCourseDTO> EnrolledCourses { get; set; }
		public int TotalEnrolledCourses { get; set; }
		public DateTime LastActivityAt { get; set; }
	}

	public class StudentEnrolledCourseDTO
	{
		public int CourseId { get; set; }
		public string CourseTitle { get; set; }
		public bool IsCompleted { get; set; }
		public double ProgressPercentage { get; set; }
		public DateTime EnrolledAt { get; set; }
	}
}
