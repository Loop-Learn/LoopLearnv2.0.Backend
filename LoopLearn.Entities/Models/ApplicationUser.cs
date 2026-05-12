using LoopLearn.Entities.Enums;
using Microsoft.AspNetCore.Identity;

namespace LoopLearn.Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName  => $"{FirstName} {FullName}";
        public string? Bio { get; set; }
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<LessonComment> LessonComments { get; set; }
        public ICollection<StudentLessonProgress> LessonProgresses { get; set; }
    }
}
