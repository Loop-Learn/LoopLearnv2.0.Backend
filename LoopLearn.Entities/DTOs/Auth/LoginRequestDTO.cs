using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Application.DTOs.Auth
{
    public class LoginRequestDTO
    {
        [Required]
        public string EmailOrUsername { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
