using LoopLearn.API.Services.Utils;
using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using System.Threading.Tasks;

namespace LoopLearn.API.Services.Courses
{
    public class CourseUpdateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageService _imageService;
        private readonly YoutubeService _youtubeService;
        public CourseUpdateService(IUnitOfWork unitOfWork, ImageService imageService, YoutubeService youtubeService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _youtubeService = youtubeService;
        }

        public async Task UpdateCourseDataAsync(Course course, UpdateCourseDTO model)
        {
            // ── Scalar Fields ─────────────────────────────
            if (model.Description is not null) course.Description = model.Description;
            if (model.ThumbnailUrl is not null)
            {
                if (!string.Equals(course.ThumbnailUrl, model.ThumbnailUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // Delete the old thumbnail file if it exists and is a local file
                    await _imageService.DeleteOldImageFileAsync(course.ThumbnailUrl);
                }
                course.ThumbnailUrl = model.ThumbnailUrl ?? "";
            }
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

            // ── Simple Collections ────────────────────────
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

            // ── Sections ──────────────────────────────────
            if (model.Sections is not null)
                await UpdateSectionsAsync(course, model.Sections);

            course.UpdatedAt = DateTime.UtcNow;
        }

        // ─────────────────────────────────────────────────
        // Sections
        // ─────────────────────────────────────────────────
        private async Task UpdateSectionsAsync(Course course, List<SectionCreationDTO> sectionDtos)
        {
            if (!sectionDtos.Any()) return;

            course.Sections ??= new List<Section>();

            var existingDbSectionIds = course.Sections.Select(s => s.Id).ToHashSet();

            var incomingExistingSectionIds = sectionDtos
                .Where(s => s.Id.HasValue && existingDbSectionIds.Contains(s.Id.Value))
                .Select(s => s.Id!.Value)
                .ToHashSet();

            var sectionsToRemove = course.Sections
                .Where(s => !incomingExistingSectionIds.Contains(s.Id))
                .ToList();

            if (sectionsToRemove.Any())
                _unitOfWork.Sections.RemoveRange(sectionsToRemove);

            foreach (var sectionDto in sectionDtos)
            {
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
                    var newSection = new Section
                    {
                        Title = sectionDto.Title ?? "Untitled Section",
                        Order = sectionDto.Order,
                        CourseId = course.Id,
                        Lessons = new List<Lesson>(),
                        Quizzes = new List<Quiz>()
                    };

                    if (sectionDto.Items is not null)
                        await AddItemsToSectionAsync(newSection, sectionDto.Items);

                    course.Sections.Add(newSection);
                }
            }
        }

        private async Task UpdateSectionItemsAsync(Section section, List<SectionItemCreationDTO> items)
        {
            if (!items.Any()) return;

            section.Lessons ??= new List<Lesson>();
            section.Quizzes ??= new List<Quiz>();

            var existingDbLessonIds = section.Lessons.Select(l => l.Id).ToHashSet();
            var existingDbQuizIds = section.Quizzes.Select(q => q.Id).ToHashSet();

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
                   await ProcessLesson(section, item.Lesson, existingDbLessonIds);
                else if (item.Type == SectionItemType.Quiz && item.Quiz is not null)
                    ProcessQuiz(section, item.Quiz, existingDbQuizIds);
            }
        }

        // ─────────────────────────────────────────────────
        // Lessons & Quizzes
        // ─────────────────────────────────────────────────
        private async Task ProcessLesson(Section section, LessonCreationDTO dto, HashSet<int> existingDbIds)
        {
            var duration = await _youtubeService.GetYouTubeVideoDurationAsync(dto.VideoUrl ?? "");
            if (dto.Id.HasValue && existingDbIds.Contains(dto.Id.Value))
            {
                var existing = section.Lessons.First(l => l.Id == dto.Id!.Value);
                if (dto.Title is not null) existing.Title = dto.Title;
                if (dto.Description is not null) existing.Description = dto.Description;
                if (dto.VideoUrl is not null)
                {
                    existing.VideoUrl = dto.VideoUrl;
                    if (duration is not null) existing.Duration = (TimeSpan) duration;
                    else if (dto.Duration is not null) existing.Duration = dto.Duration.Value;
                    else existing.Duration = TimeSpan.Zero;
                }
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
                    Duration = duration ?? (dto.Duration ?? TimeSpan.Zero),
                    Section = section,
                });
            }
        }

        private void ProcessQuiz(Section section, QuizCreationDTO dto, HashSet<int> existingDbIds)
        {
            if (dto.Id.HasValue && existingDbIds.Contains(dto.Id.Value))
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
                    CourseId = section.CourseId,
                    Questions = dto.Questions is not null
                        ? BuildQuestions(dto.Questions)
                        : new List<Question>()
                };

                section.Quizzes.Add(newQuiz);
            }
        }

        // ─────────────────────────────────────────────────
        // Questions & Options
        // ─────────────────────────────────────────────────
        private void UpdateQuestions(Quiz quiz, List<QuestionCreationDTO> dtos)
        {
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
                if (dto.Id.HasValue && existingDbIds.Contains(dto.Id.Value))
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

        private async Task AddItemsToSectionAsync(Section section, List<SectionItemCreationDTO> items)
        {
            var emptyIds = new HashSet<int>();
            foreach (var item in items)
            {
                if (item.Type == SectionItemType.Lesson && item.Lesson is not null)
                   await ProcessLesson(section, item.Lesson, emptyIds);
                else if (item.Type == SectionItemType.Quiz && item.Quiz is not null)
                    ProcessQuiz(section, item.Quiz, emptyIds);
            }
        }

        private List<Question> BuildQuestions(List<QuestionCreationDTO> dtos) =>
            dtos.Select(q => new Question
            {
                Body = q.Body ?? "",
                Points = q.Points,
                Options = q.Options is not null ? BuildOptions(q.Options) : new List<Option>()
            }).ToList();

        private List<Option> BuildOptions(List<OptionCreationDTO> dtos) =>
            dtos.Select(o => new Option
            {
                Body = o.Body ?? "",
                IsCorrect = o.IsCorrect
            }).ToList();
    }
}