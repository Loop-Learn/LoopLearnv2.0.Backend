namespace LoopLearn.Entities.DTOs.Learning
{
    public class CourseProgressDTO
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public double ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<SectionProgressDTO> Sections { get; set; } = new();
    }

    public class SectionProgressDTO
    {
        public int SectionId { get; set; }
        public string SectionTitle { get; set; }
        public int Order { get; set; }
        public List<LessonProgressItemDTO> Lessons { get; set; } = new();
        public List<QuizProgressItemDTO> Quizzes { get; set; } = new();
    }

    public class LessonProgressItemDTO
    {
        public int LessonId { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public double WatchedPercentage { get; set; }
        public int LastSecondWatched { get; set; }
    }

    public class QuizProgressItemDTO
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public bool IsPassed { get; set; }
        public int? Score { get; set; }
    }
}