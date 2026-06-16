using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Users
{
    public class InstructorApplicationDTO
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string Bio { get; set; }
        public string Expertise { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? TeachingExperience { get; set; }
        public string? CvUrl { get; set; }
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
    }

    public class SubmitInstructorApplicationDTO
    {
        [Required]
        [MinLength(50, ErrorMessage = "Bio must be at least 50 characters.")]
        public string Bio { get; set; }

        [Required]
        public string Expertise { get; set; }

        [Url(ErrorMessage = "Please provide a valid LinkedIn URL.")]
        public string? LinkedinUrl { get; set; }

        public string? TeachingExperience { get; set; }

        [Url(ErrorMessage = "Please provide a valid CV URL.")]
        public string? CvUrl { get; set; }
    }

    public class RejectApplicationDTO
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [MinLength(10, ErrorMessage = "Please provide a meaningful rejection reason.")]
        public string RejectionReason { get; set; }
    }

}