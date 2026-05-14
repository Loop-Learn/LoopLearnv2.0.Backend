namespace LoopLearn.Entities.DTOs.Course
{
    public class SectionsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public List<LessonDTO> Lessons { get; set; } = new();
        public List<QuizDTO> Quizzes { get; set; } = new();
    }
}
