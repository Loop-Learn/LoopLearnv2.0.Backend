using LoopLearn.API.Services.Enroll;
using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;

namespace LoopLearn.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Produces("application/json")]
	[Consumes("application/json")]
	public class CourseController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly EnrollmentService _enrollment;

        public CourseController(IUnitOfWork unitOfWork,EnrollmentService enrollment)
        {
            _unitOfWork = unitOfWork;
            _enrollment = enrollment;
        }


		[HttpGet("all")]
		public async Task<IActionResult> GetCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				if (page < 1 || pageSize < 1)
				{
					return BadRequest(new
					{
						success = false,
						message = "Page number and size must be greater than 0"
					});
				}
				var coursesDTOs = await _unitOfWork.Courses.GetAsync(
														predicate: c => c.Status == CourseStatus.Published,
														selector: MapToCourseCardDTO,
														includes: "Instructor,Feedbacks,Category");

				if (coursesDTOs is null || !coursesDTOs.Any())
				{
					return NoContent();
				}

				var pagedCourses = coursesDTOs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				Response.Headers.Append("Total-Count", coursesDTOs.Count().ToString());
				Response.Headers.Append("Page", page.ToString());
				Response.Headers.Append("PageSize", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page, PageSize");

				return Ok(new
				{
					success = true,
					message = $"Successfully retrieved {pagedCourses.Count} courses",
					data = pagedCourses
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An unexpected error occurred while retrieving courses."
				});
			}
		}

		[HttpGet("categories")]
		public async Task<IActionResult> GetCoursesByCategories([FromQuery] string[] categories, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				if (categories == null || categories.Length == 0)
				{
					return BadRequest(new
					{
						success = false,
						Message = "At least one category must be provided"
					});
				}

				if (page < 1 || pageSize < 1)
				{
					return BadRequest(new
					{
						success = false,
						Message = "Page number and size must be greater than 0"
					});
				}
				var categoriesInDb = await _unitOfWork.Categories.GetAsync(predicate: c => categories.Contains(c.Name),
																		   selector: c => c.Id);
				if (categoriesInDb is null || !categoriesInDb.Any())
				{
					return NotFound(new
					{
						success = false,
						message = $"No courses found for categories [{String.Join(",", categories)}]."
					});
				}
				var coursesDTOs = await _unitOfWork.Courses.GetAsync(
										predicate: c => categoriesInDb.Contains(c.CategoryId) && c.Status == CourseStatus.Published,
										selector: MapToCourseCardDTO,
										includes: "Instructor,Feedbacks");

				if (coursesDTOs is null || !coursesDTOs.Any())
				{
					return NoContent();
				}
				var pagedCourses = coursesDTOs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				Response.Headers.Append("Total-Count", coursesDTOs.Count().ToString());
				Response.Headers.Append("Page", page.ToString());
				Response.Headers.Append("PageSize", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page, PageSize");
				return Ok(new
				{
					success = true,
					message = $"Successfully retrieved {pagedCourses.Count} courses",
					data = pagedCourses
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An unexpected error occurred while retrieving courses."
				});
			}
		}

		[HttpGet("search/{searchTerm}")]
		public async Task<IActionResult> GetCoursesBySearch(string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(searchTerm))
				{
					return BadRequest(new
					{
						success = false,
						message = "Search term cannot be empty."
					});
				}

				if (page < 1 || pageSize < 1)
				{
					return BadRequest(new
					{
						success = false,
						message = "Page number and size must be greater than 0"
					});
				}

				var searchWords = searchTerm.Trim()
											.ToLower()
											.Split(new[] { ' ', ',', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries)
											.Distinct();

				var coursesCardDTOs = await _unitOfWork.Courses.GetAsync(
					predicate: c => searchWords.Any(word =>
								c.Title.ToLower().Contains(word) ||
								c.Subtitle.ToLower().Contains(word) ||
								c.CourseTags.Any(ct => ct.Tag != null && ct.Tag.Name.ToLower().Contains(word))
								&& c.Status == CourseStatus.Published
					),
					selector: MapToCourseCardDTO,
					includes: "Instructor,Feedbacks,CourseTags");

				if (coursesCardDTOs is null || !coursesCardDTOs.Any())
				{
					return NoContent();
				}

				var relevanceCourses = coursesCardDTOs.Select(c => new
				{
					Course = c,
					RelevanceScore = searchWords.Sum(word =>
						(c.Title.ToLower().Contains(word) ? 2 : 0) +
						(c.Subtitle.ToLower().Contains(word) ? 1 : 0)
					)
				}).Where(c => c.RelevanceScore > 0)
				  .OrderByDescending(c => c.RelevanceScore)
				  .ThenByDescending(c => c.Course.AverageRating)
				  .ThenBy(c => c.Course.Title)
				  .ToList();

				var pagedCourses = relevanceCourses.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				Response.Headers.Append("Total-Count", coursesCardDTOs.Count().ToString());
				Response.Headers.Append("Page", page.ToString());
				Response.Headers.Append("PageSize", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page, PageSize");
				return Ok(new
				{
					success = true,
					message = $"Successfully retrieved {pagedCourses.Count} courses",
					data = pagedCourses
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An unexpected error occurred while retrieving courses."
				});
			}
		}

		[HttpGet("{courseId}")]
		public async Task<IActionResult> GetCoursesById(int courseId)
		{
			try
			{
				if (courseId < 1)
				{
					return BadRequest(new
					{
						success = false,
						message = $"Validation error for course ID {courseId}."
					});
				}

				var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(
										predicate: c => c.Id == courseId && c.Status == CourseStatus.Published,
							 includes: "Instructor,Category,Feedbacks,Enrollments,Sections,CourseTags,CourseTags.Tag,Requirements,LearningOutcomes,Sections.Lessons,Sections.Quizzes");

				if (course is null)
				{
					return NotFound($"Course with ID {courseId} not found.");
				}

				var courseDetail = MapToCourseDetailDTO(course);
				return Ok(new
				{
					success = true,
					message = $"Course details retrieved successfully.",
					data = courseDetail
				});
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,
				new
				{
					success = false,
					message = "An unexpected error occurred while retrieving courses."
				});
			}
		}

        [HttpGet("{courseId}/feedbacks")]
        public async Task<IActionResult> GetFeedbacks(int courseId)
        {
            try
            {
                var feedback = await _unitOfWork.Feedbacks.GetAllAsync(f => f.CourseId == courseId,includes:"Student");
                if (feedback == null) return Ok(new { success = true, data = (FeedbacksDTO?)null });
				var hasPrevRate = await _unitOfWork.Feedbacks.ExistsAsync(f => f.StudentId == UserId);
				return Ok(new
				{
					success = true,
					data = MapToFeedbacksDTOs(feedback.ToList()),
					ishadRate = hasPrevRate
				});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{courseId}/feedback")]
		[Authorize]
        public async Task<IActionResult> AddFeedback(int courseId, [FromBody] FeedbackDTO dto)
        {
            try
            {
                var enrollment = await _enrollment.ValidateEnrollmentAsync(UserId, courseId);
                if (enrollment == null)
                    return Unauthorized(new { success = false, message = "You must be enrolled to leave feedback." });

                var existing = await _unitOfWork.Feedbacks.GetFirstOrDefaultAsync(f => f.StudentId == UserId && f.CourseId == courseId);
                if (existing != null)
                {
                    existing.Rating = dto.Rating;
                    existing.Comment = dto.Comment;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Feedbacks.Update(existing);
                }
                else
                {
                    await _unitOfWork.Feedbacks.AddAsync(new Feedback
                    {
                        StudentId = UserId,
                        CourseId = courseId,
                        Rating = dto.Rating,
                        Comment = dto.Comment,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _unitOfWork.SaveAsync();
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{courseId}/feedback")]
		[Authorize]
        public async Task<IActionResult> DeleteFeedback(int courseId)
        {
            try
            {
                var feedback = await _unitOfWork.Feedbacks.GetFirstOrDefaultAsync(f => f.StudentId == UserId && f.CourseId == courseId);
                if (feedback == null) return NotFound(new { success = false, message = "Feedback not found." });
                _unitOfWork.Feedbacks.Remove(feedback);
                await _unitOfWork.SaveAsync();
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        #region Mapping to DTO Methods
        private static Expression<Func<Course, CourseCardDTO>> MapToCourseCardDTO =>
			   c => new CourseCardDTO
			   {
				   Id = c.Id,
				   Title = c.Title,
				   Subtitle = c.Subtitle,
				   ThumbnailUrl = c.ThumbnailUrl,
				   InstructorName = c.Instructor != null ? c.Instructor.FullName : "Unknown",
				   AverageRating = c.Feedbacks != null && c.Feedbacks.Any()
								   ? (decimal)c.Feedbacks.Average(f => f.Rating)
								   : 0,
				   TotalRatings = c.Feedbacks != null && c.Feedbacks.Any() ? c.Feedbacks.Count() : 0,
				   Price = c.Price,
				   IsFree = c.IsFree,
				   Level = c.Level.ToString(),
				   Category = c.Category.Name
			   };
		private CourseDetailDTO MapToCourseDetailDTO(Course course)
		{
			return new CourseDetailDTO
			{
				Id = course.Id,
				Title = course.Title,
				Subtitle = course.Subtitle,
				Description = course.Description,
				ThumbnailUrl = course.ThumbnailUrl,
				Price = course.Price,
				IsFree = course.IsFree,
				Level = course.Level.ToString(),
				Language = course.Language,
				CreatedAt = course.CreatedAt,
				UpdatedAt = course.UpdatedAt,
				InstructorId = course.InstructorId,
				InstructorName = course.Instructor != null ? $"{course.Instructor.FullName}" : "Unknown",
				InstructorProfileImageUrl = course.Instructor?.ProfileImageUrl ?? "",
				InstructorBio = course.Instructor?.Bio ?? "",
				CategoryName = course.Category?.Name,
				AverageRating = (decimal)(course.Feedbacks?.Any() == true ? course.Feedbacks.Average(f => f.Rating) : 0),
				TotalRatings = course.Feedbacks?.Count ?? 0,
				EnrollmentCount = course.Enrollments?.Count ?? 0,
				SectionCount = course.Sections?.Count ?? 0,
				Tags = course.CourseTags?.Select(ct => ct.Tag?.Name).Where(t => t != null).ToList() ?? new(),
				Requirements = course.Requirements?.Select(r => r.Description).ToList() ?? new(),
				LearningOutcomes = course.LearningOutcomes?.Select(lo => lo.Description).ToList() ?? new(),
				Sections = MapToSectionsDTOs(course.Sections),
				Feedbacks = MapToFeedbacksDTOs(course.Feedbacks)
			};
		}
		private List<SectionsDTO> MapToSectionsDTOs(ICollection<Section> sections)
		{
			if (sections is null || !sections.Any())
				return new();

			return sections.Select(s => new SectionsDTO
			{
				Id = s.Id,
				Title = s.Title,
				Order = s.Order,
				Lessons = MapToLessonsDTOs(s.Lessons),
				Quizzes = MapToQuizzesDTOs(s.Quizzes)
			}).OrderBy(s => s.Order).ToList();
		}
		private List<LessonDTO> MapToLessonsDTOs(ICollection<Lesson> lessons)
		{
			if (lessons is null || !lessons.Any())
				return new();

			return lessons.Select(l => new LessonDTO
			{
				Id = l.Id,
				Title = l.Title,
				VideoURL = l.IsPreview ? l.VideoUrl : "",
				Order = l.Order,
				Duration = l.Duration,
				isPreview = l.IsPreview
			}).OrderBy(l => l.Order).ToList();
		}
		private List<QuizDTO> MapToQuizzesDTOs(ICollection<Quiz> quizzes)
		{
			if (quizzes is null || !quizzes.Any())
				return new();

			return quizzes.Select(q => new QuizDTO
			{
				Id = q.Id,
				QuizTitle = q.Title,
				Description = q.Description
			}).ToList();
		}
		private List<FeedbacksDTO> MapToFeedbacksDTOs(ICollection<Feedback> feedbacks)
		{
			if (feedbacks is null || !feedbacks.Any())
				return new();

			return feedbacks.Select(f => new FeedbacksDTO
			{
                StudentId = f.StudentId,
                Username = f.Student != null ? $"{f.Student.FullName}" : "Anonymous",
				Avatar = f.Student?.ProfileImageUrl ?? "",
				Comment = f.Comment,
				Rating = f.Rating,
				PostedAt = f.UpdatedAt ?? f.CreatedAt
			}).OrderBy(f => f.PostedAt).ThenBy(f => f.Rating).ToList();
		}
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
        #endregion

    }
}
