namespace LoopLearn.Entities.DTOs.Course
{
    public class CourseReviewHistoryDTO
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string? Comment { get; set; }
        public string PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
    }
}
