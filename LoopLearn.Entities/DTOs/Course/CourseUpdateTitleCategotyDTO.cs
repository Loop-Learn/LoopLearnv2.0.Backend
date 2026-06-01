
using LoopLearn.Entities.Helpers.CustomValidations;

namespace LoopLearn.Entities.DTOs.Course
{
	public class CourseUpdateTitleCategotyDTO
	{
		[CourseTitle]
		public string? Title { get; set; }
		public string? Category { get; set; }
	}
}
