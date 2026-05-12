namespace LoopLearn.Entities.DTOs.Course
{
    public class CourseDetailDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public decimal Price { get; set; }

        public bool IsFree { get; set; }

        public string Level { get; set; }

        public string Language { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int InstructorId { get; set; }

        public string InstructorName { get; set; }

        public string CategoryName { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalRatings { get; set; }

        public int EnrollmentCount { get; set; }

        public int SectionCount { get; set; }

        public List<string> Tags { get; set; } = new();

        public List<string> Requirements { get; set; } = new();

        public List<string> LearningOutcomes { get; set; } = new();
        public List<SectionsDTO> Sections { get; set; } = new();
        public List<FeedbacksDTO> Feedbacks { get; set; } = new();
    }
}