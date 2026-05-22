using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Helpers.CustomValidations;
using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Course
{
    public class CourseCreationDTO
    {
        [Required]
        [CourseTitle]
        public string Title { get; set; }
        [Required]
        public string Category { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
    }
}
