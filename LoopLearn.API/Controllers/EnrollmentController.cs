using LoopLearn.Entities.DTOs;
using LoopLearn.Entities.DTOs.Enrollment;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoopLearn.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class EnrollmentController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;

		public EnrollmentController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		private string GetUserId() =>
			User.FindFirstValue(ClaimTypes.NameIdentifier)
			?? throw new UnauthorizedAccessException();

		// =============================================
		// POST /api/enrollment/courses/{courseId}
		// Free courses فقط — Paid عن طريق PaymentController
		// =============================================
		[HttpPost("courses/{courseId:int}")]
		public async Task<IActionResult> Enroll(int courseId)
		{
			try
			{
				var studentId = GetUserId();

				var course = await _unitOfWork.Courses
					.GetFirstOrDefaultAsync(c => c.Id == courseId);

				if (course is null || course.IsDeleted)
					return NotFound(new { success = false, message = "Course not found." });

				if (course.Status != CourseStatus.Published)
					return BadRequest(new { success = false, message = "Course is not available for enrollment." });

				if (!course.IsFree)
					return BadRequest(new
					{
						success = false,
						message = "This is a paid course. Please use the payment endpoint.",
						paymentEndpoint = $"/api/payment/checkout"
					});

				var alreadyEnrolled = await _unitOfWork.Enrollments
					.ExistsAsync(e => e.StudentId == studentId && e.CourseId == courseId);

				if (alreadyEnrolled)
					return Conflict(new { success = false, message = "You are already enrolled in this course." });

				var enrollment = new Enrollment
				{
					StudentId = studentId,
					CourseId = courseId,
					EnrolledAt = DateTime.UtcNow,
					ProgressPercentage = 0,
					IsCompleted = false,
					LastAccessAt = DateTime.UtcNow
				};

				await _unitOfWork.Enrollments.AddAsync(enrollment);
				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = "Enrolled successfully.",
					data = new EnrollmentResultDTO
					{
						CourseId = course.Id,
						CourseTitle = course.Title,
						EnrolledAt = enrollment.EnrolledAt,
						IsFree = true
					}
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// GET /api/enrollment/courses
		// return all courses that user enrolled in
		// =============================================
		[HttpGet("courses")]
		public async Task<IActionResult> GetMyEnrollments()
		{
			try
			{
				var studentId = GetUserId();

				var studentEnrollments = await _unitOfWork.Enrollments
					.GetAsync(
						predicate: e => e.StudentId == studentId,
						selector: e => new EnrolledCourseDTO
						{
							CourseId = e.CourseId,
							Title = e.Course.Title,
							Subtitle = e.Course.Subtitle,
							ThumbnailUrl = e.Course.ThumbnailUrl,
							InstructorName = e.Course.Instructor.FullName,
							ProgressPercentage = e.ProgressPercentage,
							IsCompleted = e.IsCompleted,
							EnrolledAt = e.EnrolledAt,
							LastAccessAt = e.LastAccessAt,
							CompletedAt = e.CompletedAt,

						},
						include: "Course,Course.Instructor"
					);

				return Ok(new { success = true, data = studentEnrollments });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// DELETE /api/enrollment/courses/{courseId}
		// =============================================
		[HttpDelete("courses/{courseId:int}")]
		public async Task<IActionResult> Unenroll(int courseId)
		{
			try
			{
				var studentId = GetUserId();

				var enrollment = await _unitOfWork.Enrollments
					.GetFirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

				if (enrollment is null)
					return NotFound(new { success = false, message = "Enrollment not found." });

				_unitOfWork.Enrollments.Remove(enrollment);
				await _unitOfWork.SaveAsync();

				return Ok(new { success = true, message = "Unenrolled successfully." });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}
	}
}