namespace LoopLearn.Entities.DTOs.Learning
{
    public class LessonProgressResultDTO
    {
        public int LessonId { get; set; }
        public double WatchedPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public double CourseProgressPercentage { get; set; }
        public bool CourseJustCompleted { get; set; }
    }
}