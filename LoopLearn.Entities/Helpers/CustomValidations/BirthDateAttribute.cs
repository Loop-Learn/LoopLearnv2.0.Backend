using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class BirthDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not DateTime date)
                return new ValidationResult("Invalid date format.");

            if (date > DateTime.Today)
                return new ValidationResult("Date cannot be in the future.");

            int age = DateTime.Today.Year - date.Year;
            if (date.Date > DateTime.Today.AddYears(-age))
                age--;

            if (age < 6)
                return new ValidationResult("You must be at least 6 years old.");

            return ValidationResult.Success;
        }
    }
}