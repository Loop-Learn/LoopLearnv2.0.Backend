using LoopLearn.DataAccess.Services.Enroll;
using LoopLearn.Entities.DTOs.Enrollment;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
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
        private readonly EnrollmentService _enrollmentService;

        public EnrollmentController(IUnitOfWork unitOfWork, EnrollmentService enrollmentService)
        {
            _unitOfWork = unitOfWork;
            _enrollmentService = enrollmentService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        // =============================================
        // POST /api/enrollment/courses/{courseId}
        // Free courses only — paid courses go through PaymentController
        // =============================================
        [HttpPost("courses/{courseId:int}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            try
            {
                var studentId = GetUserId();

                var course = await _unitOfWork.Courses
                    .GetFirstOrDefaultAsync(c => c.Id == courseId, ignoreQueryFilters: true);

                if (course is null || course.IsDeleted)
                    return NotFound(new { success = false, message = "Course not found." });

                if (course.Status != CourseStatus.Published)
                    return BadRequest(new { success = false, message = "Course is not available for enrollment." });

                if (!course.IsFree)
                    return BadRequest(new
                    {
                        success = false,
                        message = "This is a paid course. Please use the payment endpoint.",
                        paymentEndpoint = "/api/payment/checkout"
                    });

                // Delegate to EnrollmentService — same logic path as paid courses
                var enrollment = await _enrollmentService.EnrollStudentAsync(studentId, courseId);

                if (enrollment is null)
                    return Conflict(new { success = false, message = "You are already enrolled in this course." });

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
        // Returns all courses the authenticated user is enrolled in
        // =============================================
        [HttpGet("courses")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            try
            {
                var studentId = GetUserId();

                var studentEnrollments = await _unitOfWork.Enrollments
                    .GetAsync(
                        predicate: e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active,
                        selector: e => new EnrolledCourseDTO
                        {
                            CourseId = e.CourseId,
                            Title = e.Course.Title,
                            Subtitle = e.Course.Subtitle,
                            ThumbnailUrl = e.Course.ThumbnailUrl,
                            InstructorName = e.Course.Instructor.FullName,
                            ProgressPercentage = e.ProgressPercentage,
                            IsCourseAvailable = !e.Course.IsDeleted,
                            IsCompleted = e.IsCompleted,
                            EnrolledAt = e.EnrolledAt,
                            LastAccessAt = e.LastAccessAt,
                            CompletedAt = e.CompletedAt
                        },
                        include: "Course,Course.Instructor",
                        ignoreQueryFilters: true
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
        // Only allowed for free courses.
        // Paid enrollments must go through a refund flow.
        // =============================================
        [HttpDelete("courses/{courseId:int}")]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            try
            {
                var studentId = GetUserId();

                var enrollment = await _unitOfWork.Enrollments
                    .GetFirstOrDefaultAsync(
                        e => e.StudentId == studentId && e.CourseId == courseId
                    );

                if (enrollment is null)
                    return NotFound(new { success = false, message = "Enrollment not found." });

                // Block unenrollment from paid courses — route through refund instead
                if (enrollment.PaymentId is not null)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Paid enrollments cannot be removed directly. Please request a refund.",
                        refundEndpoint = "/api/payment/refund"
                    });

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