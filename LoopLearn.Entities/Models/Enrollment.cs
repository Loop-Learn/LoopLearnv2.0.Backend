using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.Models
{
    public class Enrollment
    {
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Link to the payment that created this enrollment (null for free courses)
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        public double ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }

        public DateTime EnrolledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? LastAccessAt { get; set; }
    }
}