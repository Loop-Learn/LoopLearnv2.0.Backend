using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class NameAttribute : ValidationAttribute
    {
        private const string RegexPattern = @"^[A-Za-z\s-]{2,25}$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Name is required.");
            }

            string name = value.ToString().Trim();

            if (!Regex.IsMatch(name, RegexPattern))
            {
                return new ValidationResult(
                    "Name must be 2-25 characters long and can only contain (A-Z, a-z), spaces, and hyphen."
                );
            }
            int hyphenCount = name.Count(c => c == '-');
            if (hyphenCount > 1)
            {
                return new ValidationResult("Name can contain at most one hyphen.");
            }

            if (hyphenCount == 1)
            {
                int hyphenIndex = name.IndexOf('-');
                if (hyphenIndex == 0 || hyphenIndex == name.Length - 1)
                {
                    return new ValidationResult("Hyphen cannot be the first or last character.");
                }
            }
            if (Regex.IsMatch(name, @"^\s+$"))
            {
                return new ValidationResult("Name cannot consist of spaces only.");
            }
            if (Regex.IsMatch(name, @"^-+$"))
            {
                return new ValidationResult("Name cannot consist of hyphens (-) only.");
            }

            return ValidationResult.Success;
        }
    }
}