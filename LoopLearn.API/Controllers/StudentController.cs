using LoopLearn.API.Services.Enroll;
using LoopLearn.Entities.DTOs.Learning;
using LoopLearn.Entities.DTOs.Users;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoopLearn.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EnrollmentService _enrollment;
        private readonly UserManager<ApplicationUser> _userManager;
        private const double CompletionThreshold = 0.9;

        public StudentController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, EnrollmentService enrollment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _enrollment = enrollment;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        [HttpGet("courses/{courseId}/watch")]
        public async Task<IActionResult> GetWatchPageContent(int courseId)
        {
            try
            {
                var enrollment = await _enrollment.ValidateEnrollmentAsync(UserId, courseId);
                if (enrollment == null)
                    return Unauthorized(new { success = false, message = "Not enrolled." });

                var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(
                    c => c.Id == courseId,
                    includes: "Sections,Sections.Lessons,Sections.Quizzes,Sections.Quizzes.Questions"
                );
                if (course == null)
                    return NotFound(new { success = false, message = "Course not found." });

                // Get lesson progress
                var allLessonIds = course.Sections.SelectMany(s => s.Lessons).Select(l => l.Id).ToList();
                var lessonProgresses = (await _unitOfWork.LessonProgresses
                    .GetAllAsync(p => p.StudentId == UserId && allLessonIds.Contains(p.LessonId)))
                    .ToDictionary(p => p.LessonId);

                // Get quiz attempts (with answers for previous attempt review)
                var allQuizIds = course.Sections.SelectMany(s => s.Quizzes).Select(q => q.Id).ToList();
                var quizAttempts = (await _unitOfWork.QuizAttempts
                    .GetAllAsync(a => a.StudentId == UserId && allQuizIds.Contains(a.QuizId),
                        includes: "Answers,Answers.Question,Answers.Option"))
                    .ToDictionary(a => a.QuizId);

                var sectionsDto = new List<WatchSectionDTO>();
                ResumeInfoDTO? resumePoint = null;

                foreach (var section in course.Sections.OrderBy(s => s.Order))
                {
                    // Lessons
                    var lessonsDto = new List<WatchLessonDTO>();
                    foreach (var lesson in section.Lessons.OrderBy(l => l.Order))
                    {
                        var progress = lessonProgresses.GetValueOrDefault(lesson.Id);
                        var progressDto = new LessonProgressDTO
                        {
                            IsCompleted = progress?.IsCompleted ?? false,
                            WatchedPercentage = progress?.WatchedPercentage ?? 0,
                            LastSecondWatched = progress?.LastSecondWatched ?? 0
                        };

                        lessonsDto.Add(new WatchLessonDTO
                        {
                            Id = lesson.Id,
                            Title = lesson.Title,
                            Description = lesson.Description,
                            VideoUrl = lesson.VideoUrl,
                            Duration = lesson.Duration,
                            Order = lesson.Order,
                            IsPreview = lesson.IsPreview,
                            Progress = progressDto
                        });

                        if (resumePoint == null && !progressDto.IsCompleted)
                        {
                            resumePoint = new ResumeInfoDTO
                            {
                                ItemType = "Lesson",
                                ItemId = lesson.Id,
                                LastSecondWatched = progressDto.LastSecondWatched
                            };
                        }
                    }

                    // Quizzes
                    var quizzesDto = new List<WatchQuizDTO>();
                    foreach (var quiz in section.Quizzes)
                    {
                        var attempt = quizAttempts.GetValueOrDefault(quiz.Id);
                        var previousDto = attempt != null ? MapAttemptToResult(attempt) : null;

                        quizzesDto.Add(new WatchQuizDTO
                        {
                            Id = quiz.Id,
                            Title = quiz.Title,
                            Description = quiz.Description,
                            PassingScore = quiz.PassingScore,
                            TotalQuestions = quiz.Questions.Count,
                            PreviousAttempt = previousDto
                        });

                        if (resumePoint == null && (attempt == null || !attempt.IsPassed))
                        {
                            resumePoint = new ResumeInfoDTO
                            {
                                ItemType = "Quiz",
                                ItemId = quiz.Id,
                                LastSecondWatched = null
                            };
                        }
                    }

                    sectionsDto.Add(new WatchSectionDTO
                    {
                        Id = section.Id,
                        Title = section.Title,
                        Order = section.Order,
                        Lessons = lessonsDto,
                        Quizzes = quizzesDto
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new WatchCourseDTO
                    {
                        CourseId = course.Id,
                        Title = course.Title,
                        Subtitle = course.Subtitle,
                        Description = course.Description,
                        ProgressPercentage = enrollment.ProgressPercentage,
                        IsCompleted = enrollment.IsCompleted,
                        ResumePoint = resumePoint,
                        Sections = sectionsDto
                    }
                });
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

        [HttpPost("lessons/{lessonId}/progress")]
        public async Task<IActionResult> UpdateLessonProgress(int lessonId, [FromBody] UpdateLessonProgressDTO dto)
        {
            try
            {
                var lesson = await _unitOfWork.Lessons.GetFirstOrDefaultAsync(l => l.Id == lessonId, includes: "Section");
                if (lesson == null)
                    return NotFound(new { success = false, message = "Lesson not found." });

                var enrollment = await _enrollment.ValidateEnrollmentAsync(UserId, lesson.Section.CourseId);
                if (enrollment == null)
                    return Unauthorized(new { success = false, message = "Not enrolled." });

                var progress = await _unitOfWork.LessonProgresses
                    .GetFirstOrDefaultAsync(p => p.StudentId == UserId && p.LessonId == lessonId);

                double watchedPercent = dto.TotalSeconds > 0
                    ? (double)dto.LastSecondWatched / dto.TotalSeconds * 100
                    : 0;
                bool isCompleted = (progress?.IsCompleted ?? false) || watchedPercent >= CompletionThreshold * 100;

                if (progress == null)
                {
                    progress = new StudentLessonProgress
                    {
                        StudentId = UserId,
                        LessonId = lessonId,
                        WatchedPercentage = watchedPercent,
                        LastSecondWatched = dto.LastSecondWatched,
                        IsCompleted = isCompleted,
                        CompletedAt = isCompleted ? DateTime.UtcNow : null
                    };
                    await _unitOfWork.LessonProgresses.AddAsync(progress);
                }
                else
                {
                    if (dto.LastSecondWatched > progress.LastSecondWatched)
                        progress.LastSecondWatched = dto.LastSecondWatched;
                    progress.WatchedPercentage = Math.Max(progress.WatchedPercentage, watchedPercent);
                    if (!progress.IsCompleted && isCompleted)
                        progress.CompletedAt = DateTime.UtcNow;
                    progress.IsCompleted = isCompleted;
                    _unitOfWork.LessonProgresses.Update(progress);
                }

                var (newProgress, justCompleted) = await RecalculateCourseProgressAsync(UserId, lesson.Section.CourseId, enrollment);
                enrollment.ProgressPercentage = newProgress;
                enrollment.LastAccessAt = DateTime.UtcNow;
                if (justCompleted)
                {
                    enrollment.IsCompleted = true;
                    enrollment.CompletedAt = DateTime.UtcNow;
                }
                _unitOfWork.Enrollments.Update(enrollment);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    data = new LessonProgressResultDTO
                    {
                        LessonId = lessonId,
                        WatchedPercentage = Math.Round(watchedPercent, 2),
                        IsCompleted = isCompleted,
                        CourseProgressPercentage = newProgress,
                        CourseJustCompleted = justCompleted
                    }
                });
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

        [HttpGet("lessons/{lessonId}/comments")]
        public async Task<IActionResult> GetLessonComments(int lessonId)
        {
            try
            {
                var comments = await _unitOfWork.LessonComments.GetAsync<LessonComment>(
                    predicate: c => c.LessonId == lessonId && c.ParentCommentId == null,
                    includes: "Student",
                    orderBy: q => q.OrderByDescending(c => c.CreatedAt)
                );
                var result = new List<CommentResponseDTO>();
                foreach (var comment in comments)
                {
                    result.Add(await MapCommentWithReplies(comment));
                }
                return Ok(new { success = true, data = result });
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

        [HttpPost("lessons/{lessonId}/comments")]
        public async Task<IActionResult> AddComment(int lessonId, [FromBody] CreateCommentDTO dto)
        {
            try
            {
                var lesson = await _unitOfWork.Lessons.GetFirstOrDefaultAsync(l => l.Id == lessonId, includes: "Section");
                if (lesson == null) return NotFound(new { success = false, message = "Lesson not found." });
                var enrollment = await _enrollment.ValidateEnrollmentAsync(UserId, lesson.Section.CourseId);
                if (enrollment == null)
                    return Unauthorized(new { success = false, message = "You must be enrolled to comment." });

                var comment = new LessonComment
                {
                    StudentId = UserId,
                    LessonId = lessonId,
                    Comment = dto.Comment,
                    ParentCommentId = dto.ParentCommentId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonComments.AddAsync(comment);
                await _unitOfWork.SaveAsync();

                var student = await _userManager.FindByIdAsync(UserId);
                return Ok(new
                {
                    success = true,
                    data = new CommentResponseDTO
                    {
                        Id = comment.Id,
                        StudentId = UserId,
                        StudentFullName = student?.FullName ?? "",
                        Comment = comment.Comment,
                        CreatedAt = comment.CreatedAt,
                        ParentCommentId = comment.ParentCommentId
                    }
                });
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

        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDTO dto)
        {
            try
            {
                var comment = await _unitOfWork.LessonComments.GetFirstOrDefaultAsync(c => c.Id == commentId);
                if (comment == null) return NotFound(new { success = false, message = "Comment not found." });
                if (comment.StudentId != UserId) return Forbid();

                comment.Comment = dto.Comment;
                comment.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.LessonComments.Update(comment);
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

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                var comment = await _unitOfWork.LessonComments.GetFirstOrDefaultAsync(c => c.Id == commentId);
                if (comment == null)
                    return NotFound(new { success = false, message = "Comment not found." });
                if (comment.StudentId != UserId)
                    return Forbid();

                // Get all reply IDs recursively (including nested replies)
                var allIdsToDelete = await GetAllReplyIdsAsync(commentId);
                allIdsToDelete.Add(commentId);

                // Delete all comments in one batch
                var commentsToDelete = await _unitOfWork.LessonComments
                    .GetAllAsync(c => allIdsToDelete.Contains(c.Id));

                _unitOfWork.LessonComments.RemoveRange(commentsToDelete);
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

        [HttpGet("quizzes/{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            try
            {
                var quiz = await _unitOfWork.Quizzes.GetFirstOrDefaultAsync(
                    q => q.Id == quizId,
                    includes: "Questions,Questions.Options,Section.Course"
                );
                if (quiz == null) return NotFound(new { success = false, message = "Quiz not found." });

                int courseId = quiz.CourseId ?? quiz.Section?.CourseId ?? 0;
                var enrollment = await _enrollment.ValidateEnrollmentAsync(UserId, courseId);
                if (enrollment == null)
                    return Unauthorized(new { success = false, message = "Not enrolled." });

                var previousAttempt = await _unitOfWork.QuizAttempts.GetFirstOrDefaultAsync(
                    a => a.StudentId == UserId && a.QuizId == quizId,
                    includes: "Answers,Answers.Question,Answers.Option"
                );

                QuizAttemptResultDTO? previousDto = null;
                if (previousAttempt != null)
                {
                    previousDto = MapAttemptToResult(previousAttempt);
                }

                return Ok(new
                {
                    success = true,
                    data = new Entities.DTOs.Learning.QuizDTO
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Description = quiz.Description,
                        PassingScore = quiz.PassingScore,
                        TotalPoints = quiz.Questions.Sum(q => q.Points),
                        TotalQuestions = quiz.Questions.Count,
                        HasAttempt = previousAttempt != null,
                        PreviousAttempt = previousDto,
                        Questions = quiz.Questions.Select(q => new QuizQuestionDTO
                        {
                            Id = q.Id,
                            Body = q.Body,
                            Points = q.Points,
                            Options = q.Options.Select(o => new QuizOptionDTO
                            {
                                Id = o.Id,
                                Body = o.Body
                            }).ToList()
                        }).ToList()
                    }
                });
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

        [HttpPost("quizzes/{quizId}/attempt")]
        public async Task<IActionResult> SubmitQuiz(int quizId, [FromBody] SubmitQuizDTO dto)
        {
            try
            {
                var quiz = await _unitOfWork.Quizzes.GetFirstOrDefaultAsync(
                    q => q.Id == quizId,
                    includes: "Questions,Questions.Options,Section.Course"
                );
                if (quiz == null) return NotFound(new { success = false, message = "Quiz not found." });

                int courseId = quiz.CourseId ?? quiz.Section?.CourseId ?? 0;
                var enrollment = await _enrollment.ValidateEnrollmentAsync(UserId, courseId);
                if (enrollment == null)
                    return Unauthorized(new { success = false, message = "Not enrolled." });

                // Overwrite previous attempt
                var oldAttempt = await _unitOfWork.QuizAttempts.GetFirstOrDefaultAsync(a => a.StudentId == UserId && a.QuizId == quizId);
                if (oldAttempt != null)
                {
                    var oldAnswers = await _unitOfWork.StudentAnswers.GetAllAsync(sa => sa.QuizAttemptId == oldAttempt.Id);
                    if (oldAnswers.Any()) _unitOfWork.StudentAnswers.RemoveRange(oldAnswers);
                    _unitOfWork.QuizAttempts.Remove(oldAttempt);
                    await _unitOfWork.SaveAsync();
                }

                // Grade answers
                var questionMap = quiz.Questions.ToDictionary(q => q.Id);
                int earnedPoints = 0;
                int totalPoints = quiz.Questions.Sum(q => q.Points);
                var studentAnswers = new List<StudentAnswer>();
                var answerResults = new List<QuizAnswerResultDTO>();

                foreach (var submitted in dto.Answers)
                {
                    if (!questionMap.TryGetValue(submitted.QuestionId, out var question)) continue;
                    var selectedOption = question.Options.FirstOrDefault(o => o.Id == submitted.SelectedOptionId);
                    if (selectedOption == null) continue;

                    bool isCorrect = selectedOption.IsCorrect;
                    if (isCorrect) earnedPoints += question.Points;

                    studentAnswers.Add(new StudentAnswer
                    {
                        QuestionId = question.Id,
                        OptionId = selectedOption.Id,
                        IsCorrect = isCorrect
                    });

                    var correctOption = question.Options.First(o => o.IsCorrect);
                    answerResults.Add(new QuizAnswerResultDTO
                    {
                        QuestionId = question.Id,
                        QuestionBody = question.Body,
                        Points = question.Points,
                        SelectedOptionId = selectedOption.Id,
                        SelectedOptionBody = selectedOption.Body,
                        IsCorrect = isCorrect,
                        CorrectOptionId = correctOption.Id,
                        CorrectOptionBody = correctOption.Body
                    });
                }

                int score = totalPoints > 0 ? (int)Math.Round((double)earnedPoints / totalPoints * 100) : 0;
                bool isPassed = score >= quiz.PassingScore;

                var attempt = new QuizAttempt
                {
                    StudentId = UserId,
                    QuizId = quizId,
                    Score = score,
                    IsPassed = isPassed,
                    StartedAt = DateTime.UtcNow,
                    SubmittedAt = DateTime.UtcNow,
                    Answers = studentAnswers
                };
                await _unitOfWork.QuizAttempts.AddAsync(attempt);
                await _unitOfWork.SaveAsync();

                // Update course progress after quiz
                var (newProgress, justCompleted) = await RecalculateCourseProgressAsync(UserId, courseId, enrollment);
                enrollment.ProgressPercentage = newProgress;
                if (justCompleted)
                {
                    enrollment.IsCompleted = true;
                    enrollment.CompletedAt = DateTime.UtcNow;
                }
                _unitOfWork.Enrollments.Update(enrollment);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    data = new QuizAttemptResultDTO
                    {
                        AttemptId = attempt.Id,
                        Score = score,
                        IsPassed = isPassed,
                        TotalPoints = totalPoints,
                        EarnedPoints = earnedPoints,
                        SubmittedAt = attempt.SubmittedAt,
                        Answers = answerResults
                    }
                });
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

        // =============================================
        // POST /api/student/instructor-application
        // Submit a new application to become an instructor.
        // Blocked if already an Instructor/Admin, or has a Pending application.
        // Allowed if previously Rejected — creates a new application row.
        // =============================================
        [HttpPost("instructor-application")]
        public async Task<IActionResult> Apply([FromBody] SubmitInstructorApplicationDTO model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(UserId);

                if (user is null)
                    return NotFound(new { success = false, message = "User not found." });

                // Block if already an instructor or admin
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains("Instructor") || currentRoles.Contains("Admin") || currentRoles.Contains("SuperAdmin"))
                    return BadRequest(new
                    {
                        success = false,
                        message = "You are already an instructor."
                    });

                // Block if a pending application already exists
                var hasPending = await _unitOfWork.InstructorApplications
                    .ExistsAsync(a =>
                        a.StudentId == UserId &&
                        a.Status == ApplicationStatus.Pending);

                if (hasPending)
                    return Conflict(new
                    {
                        success = false,
                        message = "You already have a pending application. Please wait for it to be reviewed. "
                    });

                var application = new InstructorApplication
                {
                    StudentId = UserId,
                    Bio = model.Bio,
                    Expertise = model.Expertise,
                    LinkedinUrl = model.LinkedinUrl,
                    TeachingExperience = model.TeachingExperience,
                    CvUrl = model.CvUrl,
                    Status = ApplicationStatus.Pending,
                    AppliedAt = DateTime.UtcNow
                };

                await _unitOfWork.InstructorApplications.AddAsync(application);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    success = true,
                    message = "Your application has been submitted and is under review."
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
        // GET /api/student/instructor-applications
        // Returns the student's most recent application and its status.
        // =============================================
        [HttpGet("instructor-applications")]
        public async Task<IActionResult> GetMyApplication()
        {
            try
            {
                // Most recent application — a student may have multiple
                // (one per rejection + resubmission cycle)
                var application = await _unitOfWork.InstructorApplications
                    .GetAsync(
                        predicate: a => a.StudentId == UserId,
                        selector: a => new InstructorApplicationDTO
                        {
                            Id = a.Id,
                            StudentName = a.Student.FullName,
                            StudentEmail = a.Student.Email,
                            Bio = a.Bio,
                            Expertise = a.Expertise,
                            LinkedinUrl = a.LinkedinUrl,
                            TeachingExperience = a.TeachingExperience,
                            CvUrl = a.CvUrl,
                            Status = a.Status.ToString(),
                            RejectionReason = a.RejectionReason,
                            AppliedAt = a.AppliedAt,
                            ReviewedAt = a.ReviewedAt,
                            ReviewedBy = a.ReviewedBy != null
                                ? a.ReviewedBy.FullName
                                : null
                        },
                        includes: "Student,ReviewedBy",
                        orderBy: q => q.OrderByDescending(a => a.AppliedAt)
                    );

                var latest = application.FirstOrDefault();

                if (latest is null)
                    return NotFound(new
                    {
                        success = false,
                        message = "You have not submitted an application yet."
                    });

                return Ok(new { success = true, data = latest });
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
        private async Task<(double percentage, bool justCompleted)> RecalculateCourseProgressAsync(string studentId, int courseId, Enrollment enrollment)
        {
            var allLessons = await _unitOfWork.Lessons.GetAsync(l => l.Section.CourseId == courseId, selector: l => l.Id);
            
            var completedLessons = (await _unitOfWork.LessonProgresses.GetAllAsync(p =>
                p.StudentId == studentId && allLessons.Contains(p.LessonId) && p.IsCompleted)).Count();

            var allQuizzes = await _unitOfWork.Quizzes.GetAsync(q =>
                (q.CourseId == courseId || q.Section.CourseId == courseId), selector: q => q.Id);
            
            var passedQuizzes = (await _unitOfWork.QuizAttempts.GetAllAsync(a =>
                a.StudentId == studentId && allQuizzes.Contains(a.QuizId) && a.IsPassed)).Count();

            int totalItems = allLessons.Count() + allQuizzes.Count();
           
            if (totalItems == 0) return (0, false);
           
            int completedItems = completedLessons + passedQuizzes;
            double percentage = Math.Round((double)completedItems / totalItems * 100, 2);
            bool justCompleted = !enrollment.IsCompleted && percentage >= 100;
           
            return (percentage, justCompleted);
        }

        private async Task<CommentResponseDTO> MapCommentWithReplies(LessonComment comment)
        {
            var replies = await _unitOfWork.LessonComments.GetAllAsync(c => c.ParentCommentId == comment.Id, includes: "Student");
            var student = await _userManager.FindByIdAsync(comment.StudentId);
            var result = new CommentResponseDTO
            {
                Id = comment.Id,
                StudentId = comment.StudentId,
                StudentFullName = student?.FullName ?? "",
                StudentImage = student.ProfileImageUrl ?? "",
                Comment = comment.Comment,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId,
                Replies = new List<CommentResponseDTO>()
            };
            foreach (var reply in replies)
            {
                result.Replies.Add(await MapCommentWithReplies(reply));
            }
            return result;
        }

        private QuizAttemptResultDTO MapAttemptToResult(QuizAttempt attempt)
        {
            var totalPoints = attempt.Answers.Sum(a => a.Question.Points);
            var earnedPoints = attempt.Answers.Where(a => a.IsCorrect).Sum(a => a.Question.Points);
            return new QuizAttemptResultDTO
            {
                AttemptId = attempt.Id,
                Score = attempt.Score,
                IsPassed = attempt.IsPassed,
                TotalPoints = totalPoints,
                EarnedPoints = earnedPoints,
                SubmittedAt = attempt.SubmittedAt,
                Answers = attempt.Answers.Select(a => new QuizAnswerResultDTO
                {
                    QuestionId = a.QuestionId,
                    QuestionBody = a.Question.Body,
                    Points = a.Question.Points,
                    SelectedOptionId = a.OptionId,
                    SelectedOptionBody = a.Option.Body,
                    IsCorrect = a.IsCorrect,
                    CorrectOptionId = a.Question.Options.First(o => o.IsCorrect).Id,
                    CorrectOptionBody = a.Question.Options.First(o => o.IsCorrect).Body
                }).ToList()
            };
        }

        private async Task<List<int>> GetAllReplyIdsAsync(int parentCommentId)
        {
            var replyIds = new List<int>();
            var directReplies = await _unitOfWork.LessonComments
                .GetAsync(c => c.ParentCommentId == parentCommentId, selector: c => c.Id);

            foreach (var replyId in directReplies)
            {
                replyIds.Add(replyId);
                var childReplies = await GetAllReplyIdsAsync(replyId);
                replyIds.AddRange(childReplies);
            }
            return replyIds;
        }
        #endregion
    }
}