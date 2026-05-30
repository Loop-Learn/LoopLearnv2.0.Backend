using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Course
{
    public class RejectCourseDTO
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [MinLength(10, ErrorMessage = "Please provide a meaningful rejection reason (min 10 characters).")]
        public string Comment { get; set; }
    }

}
