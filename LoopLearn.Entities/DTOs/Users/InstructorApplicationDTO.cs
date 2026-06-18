namespace LoopLearn.Entities.DTOs.Users
{
	public class InstructorApplicationDTO
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Bio { get; set; }
		public string ProfileImageUrl { get; set; }
		public DateTime RequestedAt { get; set; }
	}
}
