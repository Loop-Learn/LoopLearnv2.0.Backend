using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Users
{
	public class UpdateUserStatusDTO
	{
		[Required]
		public bool IsBanned { get; set; }
		public string? Reason { get; set; }
	}
}
