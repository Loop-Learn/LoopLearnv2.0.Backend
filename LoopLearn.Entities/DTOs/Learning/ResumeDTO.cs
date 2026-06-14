namespace LoopLearn.Entities.DTOs.Learning
{
    public class ResumeDTO
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string ItemType { get; set; } // "Lesson" or "Quiz"
        public int ItemId { get; set; }
        public string ItemTitle { get; set; }
        public int? LastSecondWatched { get; set; } // only for lessons
        public double CourseProgressPercentage { get; set; }
    }
}