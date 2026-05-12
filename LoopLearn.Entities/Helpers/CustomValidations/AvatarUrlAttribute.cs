using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace LoopLearn.Entities.Helpers.CustomValidations
{
    public class AvatarUrlAttribute : ValidationAttribute
    {
        private const int MaxLength = 500;
        private const string ImageExtensionsPattern = @"\.(jpg|jpeg|png|gif|webp)(\?.*)?$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string url = value.ToString();

            if (url.Length > MaxLength)
                return new ValidationResult($"Avatar URL cannot exceed {MaxLength} characters.");

            if (!Regex.IsMatch(url, ImageExtensionsPattern, RegexOptions.IgnoreCase))
            {
                return new ValidationResult("Avatar must be an image file (jpg, jpeg, png, gif, webp).");
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
            {
                if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
                    return new ValidationResult("Only HTTP/HTTPS URLs or relative paths are allowed.");
            }

            return ValidationResult.Success;
        }
    }
}