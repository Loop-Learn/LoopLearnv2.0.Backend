using LoopLearn.Entities.Helpers.CustomValidations;
using LoopLearn.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Application.DTOs.Auth
{
    public class RegisterRequestDTO
    {
        [Required]
        [Name]
        public string FName { get; set; }

        [Required]
        [Name]
        public string LName { get; set; }

        [Required]
        [Username]
        public string Username { get; set; }

        [Required, StringLength(100)]
        [EmailAddress(ErrorMessage = "Email address is not Valid.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [PasswordValidation]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Phone Number is not Valid. Please make sure its an EGY phone Number.")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [BirthDate]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Gender must be Male or Female.")]
        public Gender Gender { get; set; }
    }
}
