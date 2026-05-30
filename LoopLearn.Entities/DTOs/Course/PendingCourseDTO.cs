namespace LoopLearn.Entities.DTOs.Course
{
    public class PendingCourseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ThumbnailUrl { get; set; }
        public string InstructorName { get; set; }
        public string InstructorEmail { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public DateTime? SubmittedForReviewAt { get; set; }
    }
}
