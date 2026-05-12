
namespace LoopLearn.Entities.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
