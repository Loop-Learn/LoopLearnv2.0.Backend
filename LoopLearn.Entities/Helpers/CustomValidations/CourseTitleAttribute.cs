using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class CourseTitleAttribute : ValidationAttribute
    {
        private const int MinLength = 2;
        private const int MaxLength = 60;
        private const string RegexPattern = @"^[a-zA-Z0-9\s\-_,\.:;()&]+$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Course title is required.");
            }

            string title = value.ToString().Trim();

            if (title.Length < MinLength || title.Length > MaxLength)
            {
                return new ValidationResult($"Title must be between {MinLength} and {MaxLength} characters.");
            }

            if (!Regex.IsMatch(title, RegexPattern))
            {
                return new ValidationResult(
                    "Title can only contain English letters, numbers, spaces, and: - _ , . : ; ( ) &"
                );
            }

            return ValidationResult.Success;
        }
    }
}