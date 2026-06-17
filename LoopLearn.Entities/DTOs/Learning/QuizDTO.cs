namespace LoopLearn.Entities.DTOs.Learning
{
    public class QuizDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PassingScore { get; set; }
        public int TotalPoints { get; set; }
        public int TotalQuestions { get; set; }
        public bool HasAttempt { get; set; }
        public QuizAttemptResultDTO? PreviousAttempt { get; set; }
        public List<QuizQuestionDTO> Questions { get; set; } = new();
    }

    public class QuizQuestionDTO
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public int Points { get; set; }
        public List<QuizOptionDTO> Options { get; set; } = new();
    }

    public class QuizOptionDTO
    {
        public int Id { get; set; }
        public string Body { get; set; }
    }
}