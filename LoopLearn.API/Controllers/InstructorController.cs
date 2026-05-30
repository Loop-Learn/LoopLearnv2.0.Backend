using LoopLearn.API.Services.Courses;
using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoopLearn.API.Controllers
{
	[Route("api/instructor")]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor,Admin,SuperAdmin")]
	[Produces("application/json")]
	[Consumes("application/json")]
    public class InstructorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly CourseUpdateService _courseUpdateService;

		public InstructorController(IUnitOfWork unitOfWork, CourseUpdateService courseUpdateService)
        {
            _unitOfWork = unitOfWork;
			_courseUpdateService = courseUpdateService;
        }

		private string GetUserId()
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException();
			return userId;
		}

        // =============================================
		// GET /api/instructor/courses
        // =============================================
		[HttpGet("courses")]
		public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                var instructorId = GetUserId();

				var courses = await _unitOfWork.Courses.GetAsync(
					predicate: c => c.InstructorId == instructorId,
					selector: c => new InstructorCourseDTO
					{
						Id = c.Id,
						Title = c.Title,
						Subtitle = c.Subtitle,
						ThumbnailUrl = c.ThumbnailUrl,
						Price = c.Price,
						IsFree = c.IsFree,
						Status = c.Status.ToString(),
						Level = c.Level.ToString(),
						CreatedAt = c.CreatedAt,
						UpdatedAt = c.UpdatedAt,
						EnrollmentCount = c.Enrollments != null ? c.Enrollments.Count : 0,
						AverageRating = c.Feedbacks != null && c.Feedbacks.Any()
										? Math.Round(c.Feedbacks.Average(f => (double)f.Rating), 1)
										: 0.0
					},
					include: "Enrollments,Feedbacks"
				);

				return Ok(new { success = true, data = courses });
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
		// POST /api/instructor/courses
		// =============================================
		[HttpPost("courses")]
		public async Task<IActionResult> CreateCourse([FromBody] CourseCreationDTO model)
		{
			try
			{
				var instructorId = GetUserId();

				if (!ModelState.IsValid)
                    return BadRequest(new
                    {
                        success = false,
						message = "Invalid course data.",
						errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
					});

				var category = await _unitOfWork.Categories
					.GetFirstOrDefaultAsync(c => c.Name == model.Category);

				if (category is null)
					return NotFound(new
					{
						success = false,
						message = $"Category '{model.Category}' not found."
					});

				var course = new Course
				{
					Title = model.Title,
					CategoryId = category.Id,
					InstructorId = instructorId,
					Status = CourseStatus.Draft,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
					ThumbnailUrl = "",
					Level = CourseLevel.Beginner,
					IsFree = false,
					Price = 0,
					Subtitle = "",
					Description = "",
					Language = "en"
				};

				await _unitOfWork.Courses.AddAsync(course);
				await _unitOfWork.SaveAsync();

				return StatusCode(201, new
				{
					success = true,
					message = "Course created successfully.",
					data = new
					{
						id = course.Id,
						title = course.Title,
						category = category.Name,
						status = course.Status.ToString()
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
		// PUT /api/instructor/courses/{courseId}
		// =============================================
		[HttpPut("courses/{courseId:int}")]
		public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] UpdateCourseDTO model)
		{
			try
			{
				var instructorId = GetUserId();

				var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(
					c => c.Id == courseId,
					includes: "Requirements,LearningOutcomes,TargetAudiences,CourseTags,Sections,Sections.Lessons,Sections.Quizzes,Sections.Quizzes.Questions,Sections.Quizzes.Questions.Options"
				);

				if (course is null)
					return NotFound(new { success = false, message = "Course not found." });

				if (course.InstructorId != instructorId)
					return Forbid();

				if (course.Status != CourseStatus.Draft && course.Status != CourseStatus.Rejected)
                    return BadRequest(new
                    {
                        success = false,
						message = "Only Draft or Rejected courses can be edited."
                    });

				await using var transaction = await _unitOfWork.BeginTransactionAsync();

				try
                {
					await _courseUpdateService.UpdateCourseDataAsync(course, model);

					_unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveAsync();
					await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
						message = "Course saved successfully.",
						data = new { id = course.Id, status = course.Status.ToString() }
                });
            }
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}
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
		// DELETE /api/instructor/courses/{courseId}
        // =============================================
		[HttpDelete("courses/{courseId:int}")]
		public async Task<IActionResult> DeleteCourse(int courseId)
        {
            try
            {
                var instructorId = GetUserId();

                var course = await _unitOfWork.Courses
					.GetFirstOrDefaultAsync(c => c.Id == courseId);

				if (course is null)
                    return NotFound(new { success = false, message = "Course not found." });

                if (course.InstructorId != instructorId)
                    return Forbid();

				var hasActiveEnrollments = await _unitOfWork.Enrollments
					.ExistsAsync(
						e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active,
						ignoreQueryFilters: true
					);

				if (hasActiveEnrollments)
					return BadRequest(new
                        {
						success = false,
						message = "Cannot delete a course with active enrollments."
					});

				course.IsDeleted = true;
				course.DeletedAt = DateTime.UtcNow;
				_unitOfWork.Courses.Update(course);
				await _unitOfWork.SaveAsync();

				return Ok(new { success = true, message = "Course deleted successfully." });
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
