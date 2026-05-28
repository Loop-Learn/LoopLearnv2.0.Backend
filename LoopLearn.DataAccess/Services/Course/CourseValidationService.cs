using LoopLearn.Entities.Helpers.Models;
using LoopLearn.Entities.Interfaces;

namespace LoopLearn.DataAccess.Services.Course
{
    public class CourseValidationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseValidationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CourseValidationResult> ValidateForSubmissionAsync(int courseId)
        {
            var result = new CourseValidationResult();

            // Load the course with everything needed for validation in one query
            var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(
                predicate: c => c.Id == courseId,
                includes: "Sections,Sections.Lessons"
            );

            if (course is null)
            {
                result.AddError("Course not found.");
                return result;
            }

            // ── Basic fields ──────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(course.Title))
                result.AddError("Title is required.");

            if (string.IsNullOrWhiteSpace(course.Subtitle))
                result.AddError("Subtitle is required.");

            if (string.IsNullOrWhiteSpace(course.Description))
                result.AddError("Description is required.");

            if (string.IsNullOrWhiteSpace(course.ThumbnailUrl))
                result.AddError("Thumbnail image is required.");

            if (string.IsNullOrWhiteSpace(course.Language))
                result.AddError("Language is required.");

            // ── Pricing ───────────────────────────────────────────────────
            if (!course.IsFree && course.Price <= 0)
                result.AddError("A paid course must have a price greater than zero.");

            if (course.IsFree && course.Price != 0)
                result.AddError("A free course must have a price of zero.");

            // ── Content structure ─────────────────────────────────────────
            var sections = course.Sections?.ToList() ?? new();

            if (sections.Count == 0)
            {
                result.AddError("Course must have at least one section.");
            }
            else
            {
                // Every section must have a title
                var untitledSections = sections
                    .Where(s => string.IsNullOrWhiteSpace(s.Title))
                    .ToList();

                if (untitledSections.Any())
                    result.AddError($"{untitledSections.Count} section(s) are missing a title.");

                // At least one lesson must exist across all sections
                var totalLessons = sections.Sum(s => s.Lessons?.Count ?? 0);
                if (totalLessons == 0)
                    result.AddError("Course must have at least one lesson.");

                // Every section that has lessons must not have any untitled ones
                var sectionsWithUntitledLessons = sections
                    .Where(s => s.Lessons != null &&
                                s.Lessons.Any(l => string.IsNullOrWhiteSpace(l.Title)))
                    .ToList();

                if (sectionsWithUntitledLessons.Any())
                    result.AddError("Some lessons are missing a title.");

                // Every lesson must have a video URL
                var sectionsWithMissingVideos = sections
                    .Where(s => s.Lessons != null &&
                                s.Lessons.Any(l => string.IsNullOrWhiteSpace(l.VideoUrl)))
                    .ToList();

                if (sectionsWithMissingVideos.Any())
                    result.AddError("Some lessons are missing a video.");
            }

            return result;
        }
    }
}
