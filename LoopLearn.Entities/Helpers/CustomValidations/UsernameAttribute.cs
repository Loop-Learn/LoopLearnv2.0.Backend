using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class UsernameAttribute : ValidationAttribute
    {
        private const string RegexPattern = @"^[a-zA-Z0-9_]{3,25}$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Username must be provided.");
            }

            string username = value.ToString();

            if (!Regex.IsMatch(username, RegexPattern))
            {
                return new ValidationResult(
                    "Username must be 3-25 characters long and can only contain letters (A-Z, a-z), digits (0-9), and underscores (_)."
                );
            }

            if (Regex.IsMatch(username, @"^\d+$"))
            {
                return new ValidationResult("Username cannot consist of digits only.");
            }

            if (Regex.IsMatch(username, @"^_+$"))
            {
                return new ValidationResult("Username cannot consist of underscores (_) only.");
            }

            return ValidationResult.Success;
        }
    }
}