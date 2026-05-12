
namespace LoopLearn.Entities.Models
{
    public class Question
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public int Points { get; set; }

        public int QuizId { get; set; }

        public Quiz Quiz { get; set; }

        public ICollection<Option> Options { get; set; }
    }
}
