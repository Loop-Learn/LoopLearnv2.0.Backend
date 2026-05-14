namespace LoopLearn.Entities.DTOs.Course
{
    public class FeedbacksDTO
    {
        public string Username { get; set; }
        public string Avatar { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime PostedAt { get; set; }
    }
}
