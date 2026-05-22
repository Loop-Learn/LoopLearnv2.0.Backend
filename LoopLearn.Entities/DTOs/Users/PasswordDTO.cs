using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopLearn.Entities.DTOs.Users
{
    public class PasswordDTO
    {
        [Required]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
		public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


	}
}
