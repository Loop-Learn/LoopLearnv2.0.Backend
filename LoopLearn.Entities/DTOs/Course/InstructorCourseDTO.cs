namespace LoopLearn.Entities.DTOs.Course
{
	public class InstructorCourseDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Subtitle { get; set; }
		public string ThumbnailUrl { get; set; }
		public decimal Price { get; set; }
		public bool IsFree { get; set; }
		public string Status { get; set; }
		public string Level { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public int EnrollmentCount { get; set; }
		public double AverageRating { get; set; }
	}
}