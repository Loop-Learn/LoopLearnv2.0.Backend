using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Models;

namespace LoopLearn.API.Services
{
	public class CourseMappingService
	{
		public InstructorCourseDetailDTO MapToCourseDetailDTO(Course course)
		{
			if (course == null) return null;

			return new InstructorCourseDetailDTO
			{
				Id = course.Id,
				Title = course.Title,
				Subtitle = course.Subtitle,
				Description = course.Description,
				ThumbnailUrl = course.ThumbnailUrl,
				Price = course.Price,
				IsFree = course.IsFree,
				Level = course.Level.ToString(),
				Language = course.Language,
				Status = course.Status.ToString(),
				CreatedAt = course.CreatedAt,
				UpdatedAt = course.UpdatedAt,
				Category = course.Category?.Name,
				Tags = course.CourseTags?.Select(ct => new TagsDTO { Id = ct.Tag.Id , Name = ct.Tag.Name }).Where(t => t != null).ToList() ?? new List<TagsDTO>(),
				Requirements = course.Requirements?.Select(r => r.Description).ToList() ?? new(),
				LearningOutcomes = course.LearningOutcomes?.Select(lo => lo.Description).ToList() ?? new(),
				Sections = MapToSectionDetailDTOs(course.Sections)
			};
		}

		public List<SectionCreationDTO> MapToSectionDetailDTOs(ICollection<Section> sections)
		{
			if (sections == null || !sections.Any())
				return new List<SectionCreationDTO>();

			return sections
				.OrderBy(s => s.Order)
				.Select(s => new SectionCreationDTO
				{
					Id = s.Id,
					Title = s.Title,
					Order = s.Order,
					Items = BuildSectionItems(s)
				}).ToList();
		}

		public List<SectionItemCreationDTO> BuildSectionItems(Section section)
		{
			var items = new List<SectionItemCreationDTO>();

			if (section.Lessons != null)
			{
				foreach (var lesson in section.Lessons.OrderBy(l => l.Order))
				{
					items.Add(new SectionItemCreationDTO
					{
						Type = SectionItemType.Lesson,
						Lesson = new LessonCreationDTO
						{
							Id = lesson.Id,
							Title = lesson.Title,
							Description = lesson.Description,
							VideoUrl = lesson.VideoUrl,
							Order = lesson.Order,
							IsPreview = lesson.IsPreview,
							Duration = lesson.Duration
						},
						Quiz = null
					});
				}
			}

			if (section.Quizzes != null)
			{
				foreach (var quiz in section.Quizzes)
				{
					items.Add(new SectionItemCreationDTO
					{
						Type = SectionItemType.Quiz,
						Lesson = null,
						Quiz = new QuizCreationDTO
						{
							Id = quiz.Id,
							Title = quiz.Title,
							Description = quiz.Description,
							PassingScore = quiz.PassingScore,
							IsRequired = quiz.IsRequired,
							Questions = MapToQuestionDetailDTOs(quiz.Questions)
						}
					});
				}
			}

			return items;
		}

		public List<QuestionCreationDTO> MapToQuestionDetailDTOs(ICollection<Question> questions)
		{
			if (questions == null || !questions.Any())
				return new List<QuestionCreationDTO>();

			return questions.Select(q => new QuestionCreationDTO
			{
				Id = q.Id,
				Body = q.Body,
				Points = q.Points,
				Options = q.Options?.Select(o => new OptionCreationDTO
				{
					Id = o.Id,
					Body = o.Body,
					IsCorrect = o.IsCorrect
				}).ToList() ?? new()
			}).ToList();
		}
	}
}