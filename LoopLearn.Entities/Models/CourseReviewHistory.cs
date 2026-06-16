using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.Models
{
    public class CourseReviewHistory
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public CourseReviewAction Action { get; set; }

        // Required for Rejected; optional for Approved/Submitted
        public string? Comment { get; set; }

        public string PerformedById { get; set; }
        public ApplicationUser PerformedBy { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    }
}
