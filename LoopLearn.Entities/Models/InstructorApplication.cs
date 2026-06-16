using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.Models
{
    public class InstructorApplication
    {
        public int Id { get; set; }

        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public string Bio { get; set; }
        public string Expertise { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? TeachingExperience { get; set; }
        public string? CvUrl { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        // Filled by admin on rejection
        public string? RejectionReason { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedById { get; set; }
        public ApplicationUser? ReviewedBy { get; set; }
    }
}