using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Users
{
	public class UpdateUserRoleDTO
	{
		[Required]
		public string NewRole { get; set; }
	}
}
