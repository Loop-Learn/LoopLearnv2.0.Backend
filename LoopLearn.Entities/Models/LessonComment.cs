
namespace LoopLearn.Entities.Models
{
    public class LessonComment
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public int LessonId { get; set; }

        public Lesson Lesson { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? ParentCommentId { get; set; }

        public LessonComment ParentComment { get; set; }
    }
}
