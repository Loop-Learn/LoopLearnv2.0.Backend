using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace LoopLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class CourseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public CourseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                                                        selector: MapToCourseCardDTO,
                                                        include: "Instructor,Feedbacks,Category");

                if (coursesDTOs is null || !coursesDTOs.Any())
                {
                    return NoContent();
                }

                var pagedCourses = coursesDTOs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                Response.Headers.Add("Total-Count", coursesDTOs.Count().ToString());
                Response.Headers.Add("Page", page.ToString());
                Response.Headers.Add("PageSize", pageSize.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "Total-Count, Page, PageSize");

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
                if (categoriesInDb is null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No courses found for categories [{String.Join(",", categories)}]."
                    });
                }
                var coursesDTOs = await _unitOfWork.Courses.GetAsync(
                                        predicate: c => categoriesInDb.Contains(c.CategoryId),
                                        selector: MapToCourseCardDTO,
                                        include: "Instructor,Feedbacks");

                if (coursesDTOs is null || !coursesDTOs.Any())
                {
                    return NoContent();
                }
                var pagedCourses = coursesDTOs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                Response.Headers.Add("Total-Count", coursesDTOs.Count().ToString());
                Response.Headers.Add("Page", page.ToString());
                Response.Headers.Add("PageSize", pageSize.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "Total-Count, Page, PageSize");
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
                    ),
                    selector: MapToCourseCardDTO,
                    include: "Instructor,Feedbacks,CourseTags");

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

                Response.Headers.Add("Total-Count", coursesCardDTOs.Count().ToString());
                Response.Headers.Add("Page", page.ToString());
                Response.Headers.Add("PageSize", pageSize.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "Total-Count, Page, PageSize");
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

                var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(c => c.Id == courseId,
                             "Instructor,Category,Feedbacks,Enrollments,Sections,CourseTags,Requirements,LearningOutcomes,Sections.Lessons,Sections.Quizzes");

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

        [HttpPost("create")]
        [Authorize(Roles = "Instructor,Admin,SuperAdmin")]
        public async Task<IActionResult> CreateCourse(CourseCreationDTO model)
        {
            try
            {
                // Get instructor ID from token
                var instructorId = GetUserId();

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid course data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Check if category exists
                var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(c => c.Name == model.Category);
                if (category is null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Category '{model.Category}' not found."
                    });
                }

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
                    Language = "en",

                };

                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.SaveAsync();

                return CreatedAtAction(nameof(GetCoursesById), new { courseId = course.Id }, new
                {
                    success = true,
                    message = "Course created successfully",
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
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Token."
                });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = e.Message
                    //message = "An unexpected error occurred while creating the course."
                });
            }
        }

        [HttpPut("{courseId}")]
        [Authorize(Roles = "Instructor,Admin,SuperAdmin")]
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

                if (model.Description is not null) course.Description = model.Description;
                if (model.ThumbnailUrl is not null) course.ThumbnailUrl = model.ThumbnailUrl;
                if (model.Subtitle is not null) course.Subtitle = model.Subtitle;
                if (model.Language is not null) course.Language = model.Language;
                if (model.Level is not null) course.Level = model.Level.Value;

                if (model.IsFree is not null)
                {
                    course.IsFree = model.IsFree.Value;
                    course.Price = model.IsFree.Value ? 0 : (model.Price ?? course.Price);
                }
                else if (model.Price is not null)
                {
                    course.Price = model.Price.Value;
                    course.IsFree = false;
                }

                // ── Simple Collections ───────────────────────────
                if (model.Requirements is not null)
                {
                    if (course.Requirements?.Any() == true)
                        _unitOfWork.CourseRequirements.RemoveRange(course.Requirements);

                    course.Requirements = model.Requirements
                        .Where(r => !string.IsNullOrWhiteSpace(r))
                        .Select(r => new CourseRequirement { Description = r.Trim() })
                        .ToList();
                }

                if (model.LearningOutcomes is not null)
                {
                    if (course.LearningOutcomes?.Any() == true)
                        _unitOfWork.CourseLearningOutcomes.RemoveRange(course.LearningOutcomes);

                    course.LearningOutcomes = model.LearningOutcomes
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => new CourseLearningOutcome { Description = o.Trim() })
                        .ToList();
                }

                if (model.TargetAudiences is not null)
                {
                    if (course.TargetAudiences?.Any() == true)
                        _unitOfWork.CourseTargetAudiences.RemoveRange(course.TargetAudiences);

                    course.TargetAudiences = model.TargetAudiences
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .Select(a => new CourseTargetAudience { Description = a.Trim() })
                        .ToList();
                }

                if (model.TagIds is not null)
                {
                    var validTags = await _unitOfWork.Tags
                        .GetAllAsync(t => model.TagIds.Contains(t.Id));

                    if (course.CourseTags?.Any() == true)
                        _unitOfWork.CourseTags.RemoveRange(course.CourseTags);

                    course.CourseTags = validTags
                        .Select(t => new CourseTag { TagId = t.Id })
                        .ToList();
                }

                // ── Sections ─────────────────────────────────────
                if (model.Sections is not null)
                    await UpdateSectionsAsync(course, model.Sections);

                course.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = "Course saved successfully.",
                    data = new { id = course.Id, status = course.Status.ToString() }
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Invalid Token." });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = e.Message
                });
            }
        }

        #region Update Helper Methods
        private async Task UpdateSectionsAsync(Course course, List<SectionCreationDTO> sectionDtos)
        {
            if (sectionDtos == null || !sectionDtos.Any())
                return;

            course.Sections ??= new List<Section>();

            // IDs الموجودة فعلاً في الـ DB
            var existingDbSectionIds = course.Sections.Select(s => s.Id).ToHashSet();

            // IDs اللي جت من الـ Request وموجودة فعلاً في الـ DB
            var incomingExistingSectionIds = sectionDtos
                .Where(s => s.Id.HasValue && existingDbSectionIds.Contains(s.Id.Value))
                .Select(s => s.Id!.Value)
                .ToHashSet();

            // احذف اللي مش موجود في الـ Request
            var sectionsToRemove = course.Sections
                .Where(s => !incomingExistingSectionIds.Contains(s.Id))
                .ToList();

            if (sectionsToRemove.Any())
                _unitOfWork.Sections.RemoveRange(sectionsToRemove);

            foreach (var sectionDto in sectionDtos)
            {
                // موجود في الـ DB فعلاً → Update
                bool existsInDb = sectionDto.Id.HasValue && existingDbSectionIds.Contains(sectionDto.Id.Value);

                if (existsInDb)
                {
                    var existing = course.Sections.First(s => s.Id == sectionDto.Id!.Value);

                    if (sectionDto.Title is not null) existing.Title = sectionDto.Title;
                    existing.Order = sectionDto.Order;

                    if (sectionDto.Items is not null)
                        await UpdateSectionItemsAsync(existing, sectionDto.Items);
                }
                else
                {
                    // ID وهمي من الـ Frontend أو مفيش ID → Add جديد
                    var newSection = new Section
                    {
                        Title = sectionDto.Title ?? "Untitled Section",
                        Order = sectionDto.Order,
                        CourseId = course.Id,
                        Lessons = new List<Lesson>(),
                        Quizzes = new List<Quiz>()
                    };

                    if (sectionDto.Items is not null)
                        AddItemsToSection(newSection, sectionDto.Items);

                    course.Sections.Add(newSection);
                }
            }
        }
        private async Task UpdateSectionItemsAsync(Section section, List<SectionItemCreationDTO> items)
        {
            if (items == null || !items.Any()) return;

            section.Lessons ??= new List<Lesson>();
            section.Quizzes ??= new List<Quiz>();

            // IDs الموجودة فعلاً في الـ DB
            var existingDbLessonIds = section.Lessons.Select(l => l.Id).ToHashSet();
            var existingDbQuizIds = section.Quizzes.Select(q => q.Id).ToHashSet();

            // IDs اللي جت من الـ Request وموجودة في الـ DB
            var incomingExistingLessonIds = items
                .Where(i => i.Type == SectionItemType.Lesson
                         && i.Lesson?.Id.HasValue == true
                         && existingDbLessonIds.Contains(i.Lesson.Id!.Value))
                .Select(i => i.Lesson!.Id!.Value)
                .ToHashSet();

            var incomingExistingQuizIds = items
                .Where(i => i.Type == SectionItemType.Quiz
                         && i.Quiz?.Id.HasValue == true
                         && existingDbQuizIds.Contains(i.Quiz.Id!.Value))
                .Select(i => i.Quiz!.Id!.Value)
                .ToHashSet();

            // احذف اللي اتشال
            var lessonsToRemove = section.Lessons
                .Where(l => !incomingExistingLessonIds.Contains(l.Id))
                .ToList();

            var quizzesToRemove = section.Quizzes
                .Where(q => !incomingExistingQuizIds.Contains(q.Id))
                .ToList();

            if (lessonsToRemove.Any()) _unitOfWork.Lessons.RemoveRange(lessonsToRemove);
            if (quizzesToRemove.Any()) _unitOfWork.Quizzes.RemoveRange(quizzesToRemove);

            foreach (var item in items)
            {
                if (item.Type == SectionItemType.Lesson && item.Lesson is not null)
                    ProcessLesson(section, item.Lesson, existingDbLessonIds);
                else if (item.Type == SectionItemType.Quiz && item.Quiz is not null)
                    ProcessQuiz(section, item.Quiz, existingDbQuizIds);
            }
        }
        private void ProcessLesson(Section section, LessonCreationDTO dto, HashSet<int> existingDbIds)
        {
            bool existsInDb = dto.Id.HasValue && existingDbIds.Contains(dto.Id.Value);

            if (existsInDb)
            {
                var existing = section.Lessons.First(l => l.Id == dto.Id!.Value);
                if (dto.Title is not null) existing.Title = dto.Title;
                if (dto.Description is not null) existing.Description = dto.Description;
                if (dto.VideoUrl is not null) existing.VideoUrl = dto.VideoUrl;
                if (dto.Duration is not null) existing.Duration = dto.Duration.Value;
                existing.Order = dto.Order;
                existing.IsPreview = dto.IsPreview;
            }
            else
            {
                section.Lessons.Add(new Lesson
                {
                    Title = dto.Title ?? "Untitled Lesson",
                    Description = dto.Description ?? "",
                    VideoUrl = dto.VideoUrl ?? "",
                    Order = dto.Order,
                    IsPreview = dto.IsPreview,
                    Duration = dto.Duration ?? TimeSpan.Zero,
                    Section = section,
                    QuizId = null   // ← ده اللي بيحل المشكلة
                });
            }
        }
        private void ProcessQuiz(Section section, QuizCreationDTO dto, HashSet<int> existingDbIds)
        {
            bool existsInDb = dto.Id.HasValue && existingDbIds.Contains(dto.Id.Value);

            if (existsInDb)
            {
                var existing = section.Quizzes.First(q => q.Id == dto.Id!.Value);
                if (dto.Title is not null) existing.Title = dto.Title;
                if (dto.Description is not null) existing.Description = dto.Description;
                existing.PassingScore = dto.PassingScore;
                existing.IsRequired = dto.IsRequired;

                if (dto.Questions is not null)
                    UpdateQuestions(existing, dto.Questions);
            }
            else
            {
                var newQuiz = new Quiz
                {
                    Title = dto.Title ?? "Untitled Quiz",
                    Description = dto.Description ?? "",
                    PassingScore = dto.PassingScore,
                    IsRequired = dto.IsRequired,
                    Type = QuizType.SectionQuiz,
                    Section = section,
                    CourseId = section.CourseId
                };

                newQuiz.Questions = dto.Questions is not null
                    ? BuildQuestions(dto.Questions)
                    : new List<Question>();

                section.Quizzes.Add(newQuiz);
            }
        }
        private void UpdateQuestions(Quiz quiz, List<QuestionCreationDTO> dtos)
        {
            if (dtos == null || !dtos.Any()) return;

            quiz.Questions ??= new List<Question>();

            var existingDbIds = quiz.Questions.Select(q => q.Id).ToHashSet();

            var incomingExistingIds = dtos
                .Where(d => d.Id.HasValue && existingDbIds.Contains(d.Id.Value))
                .Select(d => d.Id!.Value)
                .ToHashSet();

            var toRemove = quiz.Questions
                .Where(q => !incomingExistingIds.Contains(q.Id))
                .ToList();

            if (toRemove.Any())
                _unitOfWork.Questions.RemoveRange(toRemove);

            foreach (var dto in dtos)
            {
                bool existsInDb = dto.Id.HasValue && existingDbIds.Contains(dto.Id.Value);

                if (existsInDb)
                {
                    var existing = quiz.Questions.First(q => q.Id == dto.Id!.Value);
                    if (dto.Body is not null) existing.Body = dto.Body;
                    existing.Points = dto.Points;
                    if (dto.Options is not null) UpdateOptions(existing, dto.Options);
                }
                else
                {
                    quiz.Questions.Add(new Question
                    {
                        Body = dto.Body ?? "",
                        Points = dto.Points,
                        Options = dto.Options is not null
                            ? BuildOptions(dto.Options)
                            : new List<Option>()
                    });
                }
            }
        }
        private void UpdateOptions(Question question, List<OptionCreationDTO> dtos)
        {
            if (question.Options?.Any() == true)
                _unitOfWork.Options.RemoveRange(question.Options);

            question.Options = BuildOptions(dtos);
        }

        private List<Option> BuildOptions(List<OptionCreationDTO> dtos) =>
            dtos.Select(o => new Option
            {
                Body = o.Body ?? "",
                IsCorrect = o.IsCorrect
            }).ToList();
        private List<Question> BuildQuestions(List<QuestionCreationDTO> dtos) =>
            dtos.Select(q => new Question
            {
                Body = q.Body ?? "",
                Points = q.Points,
                Options = q.Options is not null ? BuildOptions(q.Options) : new List<Option>()
            }).ToList(); 
        private void AddItemsToSection(Section section, List<SectionItemCreationDTO> items)
        {
            if (items == null || !items.Any()) return;

            var emptyIds = new HashSet<int>(); // Section جديدة → مفيش حاجة في الـ DB

            foreach (var item in items)
            {
                if (item.Type == SectionItemType.Lesson && item.Lesson is not null)
                    ProcessLesson(section, item.Lesson, emptyIds);
                else if (item.Type == SectionItemType.Quiz && item.Quiz is not null)
                    ProcessQuiz(section, item.Quiz, emptyIds);
            }
        }
        #endregion

        #region Helper Methods
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
                Username = f.Student != null ? $"{f.Student.FullName}" : "Anonymous",
                Avatar = f.Student?.ProfileImageUrl ?? "",
                Comment = f.Comment,
                Rating = f.Rating,
                PostedAt = f.UpdatedAt ?? f.CreatedAt
            }).OrderBy(f => f.PostedAt).ThenBy(f => f.Rating).ToList();
        }
        private string GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }

            return userIdClaim;
        }
        #endregion

    }
}
