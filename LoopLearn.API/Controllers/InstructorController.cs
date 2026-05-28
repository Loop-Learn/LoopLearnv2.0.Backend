using LoopLearn.DataAccess.Services.Course;
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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Instructor,Admin,SuperAdmin")]
    public class InstructorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CourseValidationService _validationService;

        public InstructorController(IUnitOfWork unitOfWork, CourseValidationService validationService)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                      ?? throw new UnauthorizedAccessException();

        // =============================================
        // GET /api/instructor/courses/{id}/submission-check
        // Lets an instructor check if a course is ready before submitting.
        // Returns { isReady, errors[] } without changing any state.
        // =============================================
        [HttpGet("courses/{id:int}/submission-check")]
        public async Task<IActionResult> SubmissionCheck(int id)
        {
            try
            {
                var instructorId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == id);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                if (course.InstructorId != instructorId)
                    return Forbid();

                var validation = await _validationService.ValidateForSubmissionAsync(id);

                return Ok(new
                {
                    success = true,
                    data = new SubmissionCheckDTO
                    {
                        IsReady = validation.IsReady,
                        Errors = validation.Errors
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
        // POST /api/instructor/courses/{id}/submit-review
        // Submits a Draft or Rejected course for admin review.
        // Validates content, transitions status to PendingReview,
        // and records the action in CourseReviewHistory.
        // =============================================
        [HttpPost("courses/{id:int}/submit-review")]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            try
            {
                var instructorId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == id);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                // Ownership check
                if (course.InstructorId != instructorId)
                    return Forbid();

                // Workflow guard — only Draft or Rejected can be submitted
                if (!CourseWorkflowService.CanSubmitForReview(course.Status))
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot submit a course with status '{course.Status}'. " +
                                  $"Only Draft or Rejected courses can be submitted for review."
                    });

                // Content validation
                var validation = await _validationService.ValidateForSubmissionAsync(id);
                if (!validation.IsReady)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Course is not ready for submission.",
                        errors = validation.Errors
                    });

                // Transition
                course.Status = CourseStatus.PendingReview;
                course.SubmittedForReviewAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Courses.Update(course);

                // Record history
                await _unitOfWork.CourseReviewHistories.AddAsync(new CourseReviewHistory
                {
                    CourseId = course.Id,
                    Action = CourseReviewAction.Submitted,
                    Comment = null,
                    PerformedById = instructorId,
                    PerformedAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = "Course submitted for review successfully. You will be notified once reviewed."
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
        // GET /api/instructor/courses/{id}/review-history
        // Returns the full review audit trail for a course.
        // Instructor can only view their own courses' history.
        // =============================================
        [HttpGet("courses/{id:int}/review-history")]
        public async Task<IActionResult> GetReviewHistory(int id)
        {
            try
            {
                var instructorId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == id);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                if (course.InstructorId != instructorId)
                    return Forbid();

                var history = await _unitOfWork.CourseReviewHistories
                    .GetAsync(
                        predicate: h => h.CourseId == id,
                        selector: h => new CourseReviewHistoryDTO
                        {
                            Id = h.Id,
                            Action = h.Action.ToString(),
                            Comment = h.Comment,
                            PerformedBy = h.PerformedBy.FullName,
                            PerformedAt = h.PerformedAt
                        },
                        include: "PerformedBy",
                        orderBy: q => q.OrderByDescending(h => h.PerformedAt)
                    );

                return Ok(new { success = true, data = history });
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
