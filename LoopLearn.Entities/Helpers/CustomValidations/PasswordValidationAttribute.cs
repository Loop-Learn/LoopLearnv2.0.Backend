using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        private const int MinLength = 8;
        private const int MaxLength = 256;

        private const string SpecialChars = @"!#$%&'()*+,-./:;=?@[\]^_`{|}~";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Password is required.");

            if (value is not string password)
                return new ValidationResult("Password must be a string.");

            var missing = new System.Collections.Generic.List<string>();

            if (password.Length < MinLength)
                missing.Add($"at least {MinLength} characters");
            else if (password.Length > MaxLength)
                missing.Add($"no more than {MaxLength} characters");

            bool hasLower = false, hasUpper = false, hasDigit = false, hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsLower(c)) hasLower = true;
                else if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (SpecialChars.Contains(c)) hasSpecial = true;

                // Early exit if everything found
                if (hasLower && hasUpper && hasDigit && hasSpecial && password.Length <= MaxLength)
                    break;
            }

            if (!hasLower) missing.Add("a lowercase letter");
            if (!hasUpper) missing.Add("an uppercase letter");
            if (!hasDigit) missing.Add("a digit");
            if (!hasSpecial) missing.Add($"a special character (any of {SpecialChars})");

            if (missing.Any())
            {
                string message = missing.Count == 1
                    ? $"Password must contain {missing[0]}."
                    : $"Password must contain: {string.Join(", ", missing.Take(missing.Count - 1))} and {missing.Last()}.";

                return new ValidationResult(message);
            }

            return ValidationResult.Success;
        }
    }
}