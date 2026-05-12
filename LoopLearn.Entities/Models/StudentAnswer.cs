
namespace LoopLearn.Entities.Models
{
    public class StudentAnswer
    {
        public int Id { get; set; }

        public int QuizAttemptId { get; set; }

        public QuizAttempt QuizAttempt { get; set; }

        public int QuestionId { get; set; }

        public Question Question { get; set; }

        public int OptionId { get; set; }

        public Option Option { get; set; }
    }
}
