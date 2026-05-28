namespace LoopLearn.Entities.DTOs.Course
{
    public class SubmissionCheckDTO
    {
        public bool IsReady { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
