using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Helpers.CustomValidations;
using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Course
{
    public class CourseCreationDTO
    {
        [Required]
        [CourseTitle]
        public string Title { get; set; }
        [Required]
        public string Category { get; set; }
    }
    public class LessonCreationDTO
    {
        public int? Id { get; set; }       
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public int Order { get; set; }
        public bool IsPreview { get; set; }
        public TimeSpan? Duration { get; set; }
    }
    public class QuizCreationDTO
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int PassingScore { get; set; }
        public bool IsRequired { get; set; }
        public List<QuestionCreationDTO>? Questions { get; set; }
    }
    public class QuestionCreationDTO
    {
        public int? Id { get; set; }
        public string? Body { get; set; }
        public int Points { get; set; }
        public List<OptionCreationDTO>? Options { get; set; }
    }
    public class OptionCreationDTO
    {
        public int? Id { get; set; }
        public string? Body { get; set; }
        public bool IsCorrect { get; set; }
    }
    public class SectionItemCreationDTO
    {
        public SectionItemType Type { get; set; }  // Lesson or Quiz
        public LessonCreationDTO? Lesson { get; set; }
        public QuizCreationDTO? Quiz { get; set; }
    }
    public class SectionCreationDTO
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public int Order { get; set; }
        public List<SectionItemCreationDTO>? Items { get; set; }
    }
}
