namespace LoopLearn.Entities.DTOs.Learning
{
    public class LessonProgressDTO
    {
        public bool IsCompleted { get; set; }
        public double WatchedPercentage { get; set; }
        public int LastSecondWatched { get; set; }
    }
}