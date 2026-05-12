
namespace LoopLearn.Entities.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string VideoUrl { get; set; }

        public int Order { get; set; }

        public TimeSpan Duration { get; set; }

        public bool IsPreview { get; set; }

        public int SectionId { get; set; }

        public Section Section { get; set; }

        public Quiz Quiz { get; set; }

        public ICollection<StudentLessonProgress> LessonProgresses { get; set; }

        public ICollection<LessonComment> LessonComments { get; set; }
    }
}
