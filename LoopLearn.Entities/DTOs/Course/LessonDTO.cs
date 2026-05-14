namespace LoopLearn.Entities.DTOs.Course
{
    public class LessonDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public bool isPreview { get; set; }
        public string? VideoURL { get; set; }
        public int Order { get; set; }
    }
}
