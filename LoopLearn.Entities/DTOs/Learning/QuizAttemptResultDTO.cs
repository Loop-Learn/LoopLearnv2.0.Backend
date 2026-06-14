namespace LoopLearn.Entities.DTOs.Learning
{
    public class QuizAttemptResultDTO
    {
        public int AttemptId { get; set; }
        public int Score { get; set; }           // percentage 0-100
        public bool IsPassed { get; set; }
        public int TotalPoints { get; set; }
        public int EarnedPoints { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<QuizAnswerResultDTO> Answers { get; set; } = new();
    }

    public class QuizAnswerResultDTO
    {
        public int QuestionId { get; set; }
        public string QuestionBody { get; set; }
        public int Points { get; set; }
        public int SelectedOptionId { get; set; }
        public string SelectedOptionBody { get; set; }
        public bool IsCorrect { get; set; }
        public int CorrectOptionId { get; set; }
        public string CorrectOptionBody { get; set; }
    }
}