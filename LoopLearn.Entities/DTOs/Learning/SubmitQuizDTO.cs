using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Learning
{
    public class SubmitQuizDTO
    {
        [Required]
        public List<QuizAnswerDTO> Answers { get; set; } = new();
    }

    public class QuizAnswerDTO
    {
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public int SelectedOptionId { get; set; }
    }
}