
namespace LoopLearn.Entities.Models
{
    public class StudentLessonProgress
    {
        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public int LessonId { get; set; }

        public Lesson Lesson { get; set; }

        public bool IsCompleted { get; set; }

        public double WatchedPercentage { get; set; }

        public int LastSecondWatched { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
