namespace LoopLearn.Entities.DTOs.Users
{
	public class AdminUserDTO
	{
		public string Id { get; set; }
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }
		public string ProfileImageUrl { get; set; }
		public bool IsLocked { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
	}
}