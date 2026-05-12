
namespace LoopLearn.Entities.Models
{
    public class Enrollment
    {
        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public double ProgressPercentage { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime EnrolledAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? LastAccessAt { get; set; }
    }
}
