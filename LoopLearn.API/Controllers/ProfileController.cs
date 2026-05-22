using LoopLearn.DataAccess.Implementation;
using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.DTOs.Users;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LoopLearn.API.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ProfileController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<ApplicationUser> _userManager;
		public ProfileController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
		}

		[HttpGet("profile")]
		public async Task<IActionResult> GetProfile()
		{
			try
			{
				var studentId = GetUserId();
				var user = await _userManager.FindByIdAsync(studentId);
				if (user is null)
					return NotFound(new
					{
						success = false,
						message = $"Student with ID {studentId} not found."
					});

				var profileDTO = MapToProfileDTO(user);
				return Ok(new
				{
					success = true,
					message = "Retrieved user profile successfully.",
					data = profileDTO
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new
				{
					success = false,
					message = "Invalid Token."
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An error occurred while fetching profile"
				});
			}
		}

		[HttpPut("profile/update")]
		public async Task<IActionResult> UpdateProfile(UpdateProfileDTO model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var studentId = GetUserId();

				var user = await _userManager.FindByIdAsync(studentId);

				if (user is null)
					return NotFound(new
					{
						success = false,
						message = $"Student with ID {studentId} not found."
					});

				var hasChaged = false;
				if (!string.IsNullOrEmpty(model.Email))
				{
					var userByEmail = await _userManager.FindByEmailAsync(model.Email);

					if (userByEmail != null && userByEmail.Id != studentId)
					{
						return BadRequest(new
						{
							success = false,
							message = "Email is already in use by another account."
						});
					}

					user.Email = model.Email;
					user.EmailConfirmed = false;
					hasChaged = true;
				}

				if (!string.IsNullOrEmpty(model.Phone) && model.Phone != user.PhoneNumber)
				{
					user.PhoneNumber = model.Phone;
					user.PhoneNumberConfirmed = false;
					hasChaged = true;
				}

				if (!string.IsNullOrEmpty(model.FirstName))
				{
					user.FirstName = model.FirstName;
					hasChaged = true;
				}

				if (!string.IsNullOrEmpty(model.LastName))
				{
					user.LastName = model.LastName;
					hasChaged = true;
				}
				if (!hasChaged)
				{
					return BadRequest(new
					{
						success = false,
						message = "No changes detected in the profile data.",
						data = MapToProfileDTO(user)
					});
				}

				var result = await _userManager.UpdateAsync(user);

				if (!result.Succeeded)
				{
					return StatusCode(StatusCodes.Status500InternalServerError, new
					{
						success = false,
						message = "Failed to update profile",
						errors = result.Errors.Select(e => e.Description)
					});
				}

				user = await _userManager.FindByIdAsync(studentId);

				var profileDTO = MapToProfileDTO(user);

				return Ok(new
				{
					success = true,
					message = "User data Updated Successfully.",
					data = profileDTO
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new
				{
					success = false,
					message = "Invalid Token."
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An error occurred while updating profile"
				});
			}

		}

		[HttpPut("changepassword")]
		public async Task<IActionResult> ChangePassword([FromBody] PasswordDTO model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var studentId = GetUserId();

				var user = await _userManager.FindByIdAsync(studentId);

				if (user is null)
					return NotFound(new
					{
						success = false,
						message = $"Student with ID {studentId} not found."
					});

				if (!await _userManager.CheckPasswordAsync(user, model.OldPassword))
				{
					return BadRequest(new
					{
						success = false,
						message = "Current password is incorrect."
					});
				}

				if (model.NewPassword == model.OldPassword)
				{
					return BadRequest(new
					{
						success = false,
						message = "New password must be different from old password."
					});
				}


				var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

				if (!result.Succeeded)
				{
					return BadRequest(new
					{
						success = false,
						message = "Failed to change password",
						errors = result.Errors.Select(e => e.Description)
					});
				}

				return Ok(new
				{
					success = true,
					message = "Password changed successfully."
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new
				{
					success = false,
					message = "Invalid Token."
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An error occurred while updating profile"
				});
			}
		}


		#region Helper Methods
		private ProfileDTO MapToProfileDTO(ApplicationUser user) => new ProfileDTO
		{
			Avatar = user.ProfileImageUrl,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Username = user.UserName,
			Email = user.Email,
			IsVerifiedEmail = user.EmailConfirmed,
			Phone = user.PhoneNumber,
			IsVerifiedPhone = user.PhoneNumberConfirmed,
			Gender = user.Gender,
			JoinDate = user.CreatedAt,
			BirthDate = user.BirthDate
		};
		private string GetUserId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (String.IsNullOrEmpty(userIdClaim))
			{
				throw new UnauthorizedAccessException();
			}

			return userIdClaim;
		}
		#endregion
	}
}
