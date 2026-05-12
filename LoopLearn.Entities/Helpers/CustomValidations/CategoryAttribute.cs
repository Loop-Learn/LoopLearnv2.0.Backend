using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class CategoryAttribute : ValidationAttribute
    {
        private const int MinLength = 2;
        private const int MaxLength = 50;
        private const string RegexPattern = @"^[a-zA-Z\s\-]+$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Category is required.");
            }

            string category = value.ToString().Trim();

            if (category.Length < MinLength || category.Length > MaxLength)
            {
                return new ValidationResult($"Category must be between {MinLength} and {MaxLength} characters.");
            }

            if (!Regex.IsMatch(category, RegexPattern))
            {
                return new ValidationResult(
                    "Category can only contain English letters, spaces, and hyphens (e.g., 'Web Development', 'Data-Science')."
                );
            }

            if (Regex.IsMatch(category, @"^\s+$"))
            {
                return new ValidationResult("Category cannot consist of spaces only.");
            }
            if (Regex.IsMatch(category, @"^-+$"))
            {
                return new ValidationResult("Category cannot consist of hyphens (-) only.");
            }

            return ValidationResult.Success;
        }
    }
}