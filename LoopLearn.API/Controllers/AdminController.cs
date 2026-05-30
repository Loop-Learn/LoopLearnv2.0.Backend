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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                      ?? throw new UnauthorizedAccessException();

        // =============================================
        // GET /api/admin/courses/pending
        // Returns all courses awaiting review, ordered by submission date
        // (oldest first — review in the order they came in).
        // =============================================
        [HttpGet("courses/pending")]
        public async Task<IActionResult> GetPendingCourses()
        {
            try
            {
                var pendingCourses = await _unitOfWork.Courses
                    .GetAsync(
                        predicate: c => c.Status == CourseStatus.PendingReview && !c.IsDeleted,
                        selector: c => new PendingCourseDTO
                        {
                            Id = c.Id,
                            Title = c.Title,
                            Subtitle = c.Subtitle,
                            ThumbnailUrl = c.ThumbnailUrl,
                            InstructorName = c.Instructor.FullName,
                            InstructorEmail = c.Instructor.Email ?? "Email Not Provided.",
                            Category = c.Category.Name,
                            Price = c.Price,
                            IsFree = c.IsFree,
                            SubmittedForReviewAt = c.SubmittedForReviewAt
                        },
                        include: "Instructor,Category",
                        orderBy: q => q.OrderBy(c => c.SubmittedForReviewAt)
                    );

                return Ok(new { success = true, data = pendingCourses });
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
        // POST /api/admin/courses/{id}/approve
        // Publishes a PendingReview course.
        // Only PendingReview courses can be approved —
        // Draft/Rejected/Archived cannot be approved directly.
        // =============================================
        [HttpPost("courses/{id:int}/approve")]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            try
            {
                var adminId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == id);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                if (!CourseWorkflowService.CanApprove(course.Status))
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot approve a course with status '{course.Status}'. " +
                                  $"Only PendingReview courses can be approved."
                    });

                course.Status = CourseStatus.Published;
                course.PublishedAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Courses.Update(course);

                await _unitOfWork.CourseReviewHistories.AddAsync(new CourseReviewHistory
                {
                    CourseId = course.Id,
                    Action = CourseReviewAction.Approved,
                    Comment = null,
                    PerformedById = adminId,
                    PerformedAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Course '{course.Title}' has been approved and is now published."
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
        // POST /api/admin/courses/{id}/reject
        // Rejects a PendingReview course with a required comment.
        // Status moves to Rejected — instructor can fix and re-submit.
        // =============================================
        [HttpPost("courses/{id:int}/reject")]
        public async Task<IActionResult> RejectCourse(int id, [FromBody] RejectCourseDTO model)
        {
            try
            {
                var adminId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == id);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                if (!CourseWorkflowService.CanReject(course.Status))
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot reject a course with status '{course.Status}'. " +
                                  $"Only PendingReview courses can be rejected."
                    });

                course.Status = CourseStatus.Rejected;
                course.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Courses.Update(course);

                await _unitOfWork.CourseReviewHistories.AddAsync(new CourseReviewHistory
                {
                    CourseId = course.Id,
                    Action = CourseReviewAction.Rejected,
                    Comment = model.Comment,
                    PerformedById = adminId,
                    PerformedAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Course '{course.Title}' has been rejected.",
                    rejectionReason = model.Comment
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
        // GET /api/admin/courses/{id}/review-history
        // Full audit trail — admins can view history for any course.
        // =============================================
        [HttpGet("courses/{id:int}/review-history")]
        public async Task<IActionResult> GetReviewHistory(int id)
        {
            try
            {
                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == id);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

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
