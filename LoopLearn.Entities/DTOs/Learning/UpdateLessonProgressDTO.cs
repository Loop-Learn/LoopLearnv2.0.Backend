using System.ComponentModel.DataAnnotations;

namespace LoopLearn.Entities.DTOs.Learning
{
    public class UpdateLessonProgressDTO
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int LastSecondWatched { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalSeconds { get; set; }
    }
}