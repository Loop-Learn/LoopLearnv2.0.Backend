using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public decimal Price { get; set; }

        public bool IsFree { get; set; }

        public CourseLevel Level { get; set; }

        public string Language { get; set; }

        public CourseStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string InstructorId { get; set; }

        public ApplicationUser Instructor { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public ICollection<Section> Sections { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; }

        public ICollection<CourseRequirement> Requirements { get; set; }

        public ICollection<CourseLearningOutcome> LearningOutcomes { get; set; }

        public ICollection<CourseTargetAudience> TargetAudiences { get; set; }

        public ICollection<CourseTag> CourseTags { get; set; }
    }
}
