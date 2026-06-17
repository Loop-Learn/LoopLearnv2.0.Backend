namespace LoopLearn.Entities.DTOs.Course
{
    public class FeedbacksDTO
    {
        public string StudentId { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime PostedAt { get; set; }
    }
    public class FeedbackDTO
    {
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
    }
}
