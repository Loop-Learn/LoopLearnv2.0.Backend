namespace LoopLearn.Entities.DTOs.Users
{
	public class AdminUserDetailDTO
	{
		public string Id { get; set; }
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }
		public string? Bio { get; set; }
		public string? ProfileImageUrl { get; set; }
		public bool IsLocked { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }

		// for students 
		public int? EnrollmentCount { get; set; }

		// for instructors 
		public int? CourseCount { get; set; }
	}
}