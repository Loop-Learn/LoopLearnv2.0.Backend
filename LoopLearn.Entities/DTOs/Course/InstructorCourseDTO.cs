using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.DTOs.Course
{
    public class InstructorCourseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
