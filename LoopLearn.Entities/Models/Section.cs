
namespace LoopLearn.Entities.Models
{
    public class Section
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
    }
}
