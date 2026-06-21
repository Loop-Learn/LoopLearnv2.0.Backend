using LoopLearn.Entities.Helpers.CustomValidations;
using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Users
{
    public class UpdateProfileDTO
    {
        [Name]
        public string? FirstName { get; set; }
        [Name]
        public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Phone Number is not Valid. Please make sure its an EGY phone Number.")]
        public string? Phone { get; set; }
		public string Bio { get; set; }
	}
}
