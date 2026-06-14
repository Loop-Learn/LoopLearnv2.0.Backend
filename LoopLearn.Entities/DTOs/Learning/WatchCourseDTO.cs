namespace LoopLearn.Entities.DTOs.Learning
{
    public class WatchCourseDTO
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public double ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public ResumeInfoDTO? ResumePoint { get; set; }
        public List<WatchSectionDTO> Sections { get; set; } = new();
    }

    public class WatchSectionDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public List<WatchLessonDTO> Lessons { get; set; } = new();
        public List<WatchQuizDTO> Quizzes { get; set; } = new();
    }

    public class WatchLessonDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public int Order { get; set; }
        public bool IsPreview { get; set; }
        public LessonProgressDTO Progress { get; set; }
    }

    public class WatchQuizDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PassingScore { get; set; }
        public int TotalQuestions { get; set; }
        public QuizAttemptResultDTO? PreviousAttempt { get; set; }
    }

    public class ResumeInfoDTO
    {
        public string ItemType { get; set; } // "Lesson" or "Quiz"
        public int ItemId { get; set; }
        public int? LastSecondWatched { get; set; } // only for lessons
    }
}