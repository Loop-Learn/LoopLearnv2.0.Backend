
namespace LoopLearn.Entities.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public ApplicationUser Student { get; set; }

        public int QuizId { get; set; }

        public Quiz Quiz { get; set; }

        public int Score { get; set; }

        public bool IsPassed { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime SubmittedAt { get; set; }

        public ICollection<StudentAnswer> Answers { get; set; }
    }
}
