using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public QuizType Type { get; set; }

        public int PassingScore { get; set; }

        public bool IsRequired { get; set; }

        public int? SectionId { get; set; }

        public Section Section { get; set; }

        public int? CourseId { get; set; }

        public Course Course { get; set; }

        public ICollection<Question> Questions { get; set; }
    }
}
