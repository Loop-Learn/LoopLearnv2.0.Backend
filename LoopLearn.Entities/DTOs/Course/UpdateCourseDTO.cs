using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.DTOs.Course
{
	public class UpdateCourseDTO
	{
		public string? Description { get; set; }
		public string? ThumbnailUrl { get; set; }
		public string? Subtitle { get; set; }
		public string? Language { get; set; }
		public CourseLevel? Level { get; set; }
		public bool? IsFree { get; set; }
		public decimal? Price { get; set; }
		public List<string>? Requirements { get; set; }
		public List<string>? LearningOutcomes { get; set; }
		public List<string>? TargetAudiences { get; set; }
		public List<int>? TagIds { get; set; }
		public List<SectionCreationDTO>? Sections { get; set; }
	}
}