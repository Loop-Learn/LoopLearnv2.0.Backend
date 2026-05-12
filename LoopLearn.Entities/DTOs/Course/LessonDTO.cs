namespace LoopLearn.Entities.DTOs.Course
{
    public class LessonDTO
    {
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public bool isPreview { get; set; }
        public string? VideoURL { get; set; }
        public int Order { get; set; }
    }
}
