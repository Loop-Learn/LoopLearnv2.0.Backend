using LoopLearn.API.Services.Courses;
using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoopLearn.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Instructor,Admin,SuperAdmin")]
	[Produces("application/json")]
	[Consumes("application/json")]
	public class InstructorController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly CourseUpdateService _courseUpdateService;
		private readonly CourseValidationService _validationService;


		public InstructorController(IUnitOfWork unitOfWork, CourseUpdateService courseUpdateService, CourseValidationService courseValidationService)
		{
			_unitOfWork = unitOfWork;
			_courseUpdateService = courseUpdateService;
			_validationService = courseValidationService;
		}

        private string GetUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();
            return userId;
        }

		// =============================================
		// GET /api/instructor/students
		// =============================================
		[HttpGet("students")]
		public async Task<IActionResult> GetMyStudents(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] int? courseId = null)
		{
			try
			{
				if (page < 1 || pageSize < 1)
					return BadRequest(new { success = false, message = "Page and pageSize must be greater than 0." });

				var instructorId = GetUserId();

				var enrollments = await _unitOfWork.Enrollments.GetAllAsync(
					predicate: e => e.Course.InstructorId == instructorId
								 && e.Status == EnrollmentStatus.Active
								 && (courseId == null || e.CourseId == courseId),
					includes: "Student,Course"
				);

				if (enrollments == null || !enrollments.Any())
					return NoContent();

				var students = enrollments
					.GroupBy(e => e.StudentId)
					.Select(g => new InstructorStudentDTO
					{
						StudentId = g.Key,
						FullName = g.First().Student.FullName,
						Email = g.First().Student.Email ?? string.Empty,
						ProfileImageUrl = g.First().Student.ProfileImageUrl ?? string.Empty,
						TotalEnrolledCourses = g.Count(),
						LastActivityAt = g.Max(e => e.LastAccessAt ?? e.EnrolledAt),
						EnrolledCourses = g.Select(e => new StudentEnrolledCourseDTO
						{
							CourseId = e.CourseId,
							CourseTitle = e.Course.Title,
							ProgressPercentage = e.ProgressPercentage,
							IsCompleted = e.IsCompleted,
							EnrolledAt = e.EnrolledAt
						}).ToList()
					})
					.OrderByDescending(s => s.LastActivityAt)
					.ToList();

				var totalStudents = students.Count;

				var pagedStudents = students
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				// Header metadata
				Response.Headers.Append("Total-Count", totalStudents.ToString());
				Response.Headers.Append("Page-Number", page.ToString());
				Response.Headers.Append("Page-Size", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page-Number, Page-Size");

				return Ok(new { success = true, data = pagedStudents });
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
		// GET /api/instructor/courses
		// =============================================
		[HttpGet("courses")]
		public async Task<IActionResult> GetMyCourses(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] CourseStatus? courseStatus = null)
		{
			try
			{
				if (page <= 0 || pageSize <= 0)
					return BadRequest(new
					{
						success = false,
						message = "Page number and size must be greater than 0"
					});

				var instructorId = GetUserId();

				var courses = await _unitOfWork.Courses.GetAsync(
					predicate: c => (courseStatus == null) ?
										c.InstructorId == instructorId :
										c.InstructorId == instructorId && c.Status == courseStatus,
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
					include: "Enrollments,Feedbacks",
					orderBy: q => q.OrderByDescending(c => c.CreatedAt)
				);

				if (courses == null || !courses.Any())
					return StatusCode(204, new
					{
						success = true,
						message = "No courses found for the instructor."
					});

				var pagedCourses = courses
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				// Stats 
				var totalCourses = courses.Count();

				var totalEnrollments = courses.Sum(c => c.EnrollmentCount);

				var stuedents = await _unitOfWork.Enrollments
					.GetAsync(
						predicate: e => e.Course.InstructorId == instructorId,
						selector: e => e.StudentId
					);
				var uniqueStudents = stuedents.Distinct().Count();

				var payments = await _unitOfWork.Payments
					.GetAsync(
						predicate: p => p.Course.InstructorId == instructorId && p.Status == PaymentStatus.Succeeded,
						selector: p => p.Amount
					);
				var totalRevenue = payments.Sum();

				// Header metadata
				Response.Headers.Append("Total-Count", totalCourses.ToString());
				Response.Headers.Append("Total-Enrollments", totalEnrollments.ToString());
				Response.Headers.Append("Total-Students", uniqueStudents.ToString());
				Response.Headers.Append("Total-Revenue", totalRevenue.ToString("F2"));
				Response.Headers.Append("Page-Number", page.ToString());
				Response.Headers.Append("Page-Size", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Total-Enrollments, Total-Students, Total-Revenue, Page-Number, Page-Size");

				return Ok(new { success = true, data = pagedCourses });
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
        // GET /api/instructor/courses/id
        // =============================================
        [HttpGet("courses/{courseId}")]
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

                var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(c => c.Id == courseId,
                             "Category,Instructor,Sections,CourseTags,Requirements,LearningOutcomes,Sections.Lessons,Sections.Quizzes,Sections.Quizzes.Questions,Sections.Quizzes.Questions.Options");

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
		// PATCH /api/instructor/courses/{courseId}/title-category
		// =============================================
		[HttpPatch("courses/{courseId:int}/title-category")]
		public async Task<IActionResult> UpdateTitleAndCategory(int courseId, [FromBody] CourseUpdateTitleCategotyDTO model)
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

				var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(c => c.Id == courseId);

				if (course is null || course.IsDeleted)
					return NotFound(new { success = false, message = "Course not found." });

				if (course.InstructorId != instructorId)
					return Forbid();

				if (course.Status != CourseStatus.Draft && course.Status != CourseStatus.Rejected)
					return BadRequest(new
					{
						success = false,
						message = "Only Draft or Rejected courses can be edited."
					});

				if (string.IsNullOrEmpty(model.Title) && string.IsNullOrEmpty(model.Category))
					return BadRequest(new
					{
						success = false,
						message = "At least one field (Title or Category) must be provided for update."
					});

				if (!string.IsNullOrEmpty(model.Category))
				{
					var category = await _unitOfWork.Categories
						.GetFirstOrDefaultAsync(c => c.Name == model.Category);
					if (category is null)
						return NotFound(new
						{
							success = false,
							message = $"Category '{model.Category}' not found."
						});
					course.CategoryId = category.Id;
				}

				if (!string.IsNullOrEmpty(model.Title))
					course.Title = model.Title;

				course.UpdatedAt = DateTime.UtcNow;

				_unitOfWork.Courses.Update(course);
				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = "Course title and category updated successfully.",
					data = new
					{
						id = course.Id,
						title = course.Title,
						categoryId = model.Category ?? "Unchanged",
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

        // =============================================
        // POST /api/instructor/courses/{id}/submit-review
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
                    Action = CourseStatus.Submitted,
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


        #region Helper Methods
        private InstructorCourseDetailDTO MapToCourseDetailDTO(Course course)
        {
            return new InstructorCourseDetailDTO
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
                Category = course.Category?.Name,
                Tags = course.CourseTags?.Select(ct => ct.Tag?.Name).Where(t => t != null).ToList() ?? new(),
                Requirements = course.Requirements?.Select(r => r.Description).ToList() ?? new(),
                LearningOutcomes = course.LearningOutcomes?.Select(lo => lo.Description).ToList() ?? new(),
                Sections = MapToSectionDetailDTOs(course.Sections)   // New method
            };
        }

        private List<SectionCreationDTO> MapToSectionDetailDTOs(ICollection<Section> sections)
        {
            if (sections == null || !sections.Any())
                return new List<SectionCreationDTO>();

            return sections.OrderBy(s => s.Order).Select(s => new SectionCreationDTO
            {
                Id = s.Id,
                Title = s.Title,
                Order = s.Order,
                Items = BuildSectionItems(s)
            }).ToList();
        }
        private List<SectionItemCreationDTO> BuildSectionItems(Section section)
        {
            var items = new List<SectionItemCreationDTO>();

            // Add lessons
            if (section.Lessons != null)
            {
                foreach (var lesson in section.Lessons.OrderBy(l => l.Order))
                {
                    items.Add(new SectionItemCreationDTO
                    {
                        Type = SectionItemType.Lesson,
                        Lesson = new LessonCreationDTO
                        {
                            Id = lesson.Id,
                            Title = lesson.Title,
                            Description = lesson.Description,
                            VideoUrl = lesson.VideoUrl,     // Always return the real URL
                            Order = lesson.Order,
                            IsPreview = lesson.IsPreview,
                            Duration = lesson.Duration
                        },
                        Quiz = null
                    });
                }
            }

            // Add quizzes
            if (section.Quizzes != null)
            {
                foreach (var quiz in section.Quizzes)
                {
                    items.Add(new SectionItemCreationDTO
                    {
                        Type = SectionItemType.Quiz,
                        Lesson = null,
                        Quiz = new QuizCreationDTO
                        {
                            Id = quiz.Id,
                            Title = quiz.Title,
                            Description = quiz.Description,
                            PassingScore = quiz.PassingScore,
                            IsRequired = quiz.IsRequired,
                            Questions = MapToQuestionDetailDTOs(quiz.Questions)
                        }
                    });
                }
            }

            return items;
        }
        private List<QuestionCreationDTO> MapToQuestionDetailDTOs(ICollection<Question> questions)
        {
            if (questions == null || !questions.Any())
                return new List<QuestionCreationDTO>();

            return questions.Select(q => new QuestionCreationDTO
            {
                Id = q.Id,
                Body = q.Body,
                Points = q.Points,
                Options = q.Options?.Select(o => new OptionCreationDTO
                {
                    Id = o.Id,
                    Body = o.Body,
                    IsCorrect = o.IsCorrect
                }).ToList() ?? new()
            }).ToList();
        }
        #endregion

    }
}
