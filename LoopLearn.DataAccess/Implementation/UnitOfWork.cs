using LoopLearn.DataAccess.Data;
using LoopLearn.DataAccess.Implementation.Repositories;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoopLearn.DataAccess.Implementation
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		ICourseRepository _courses;
		IEnrollmentRepository _enrollments;
		ILessonProgressRepository _lessonProgresses;
		IQuizRepository _quizzes;
		IQuizAttemptRepository _quizAttempts;
		IFeedbackRepository _feedbacks;
		ICategoryRepository _categories;
		ITagRepository _tags;
		ICourseRequirementRepository _courseRequirements;
		ICourseLearningOutcomeRepository _courseLearningOutcomes;
		ICourseTargetAudienceRepository _courseTargetAudiences;
		ICourseTagRepository _courseTags;
		ISectionRepository _sections;
		ILessonRepository _lessons;
		IQuestionRepository _questions;
		IOptionRepository _options;
		IPaymentRepository _payments;
		ICourseReviewHistoryRepository _courseReviewHistoryRepository;
		ILessonCommentRepository _lessonComments;
		IStudentAnswersRepository _studentAnswers;

        public UnitOfWork(ApplicationDbContext context)
		{
			_context = context;
		}

		public ICourseRepository Courses
		{
			get
			{
				if (_courses is null)
				{
					_courses = new CourseRepository(_context);
				}

				return _courses;
			}
			private set { _courses = value; }
		}

		public IEnrollmentRepository Enrollments
		{
			get
			{
				if (_enrollments is null)
				{
					_enrollments = new EnrollmentRepository(_context);
				}

				return _enrollments;
			}
			private set { _enrollments = value; }
		}

		public ILessonProgressRepository LessonProgresses
		{
			get
			{
				if (_lessonProgresses is null)
				{
					_lessonProgresses = new LessonProgressRepository(_context);
				}

				return _lessonProgresses;
			}
			private set { _lessonProgresses = value; }
		}

		public IQuizRepository Quizzes
		{
			get
			{
				if (_quizzes is null)
				{
					_quizzes = new QuizRepository(_context);
				}

				return _quizzes;
			}
			private set { _quizzes = value; }
		}

		public IQuizAttemptRepository QuizAttempts
		{
			get
			{
				if (_quizAttempts is null)
				{
					_quizAttempts = new QuizAttemptRepository(_context);
				}

				return _quizAttempts;
			}
			private set { _quizAttempts = value; }
		}

		public IFeedbackRepository Feedbacks
		{
			get
			{
				if (_feedbacks is null)
				{
					_feedbacks = new FeedbackRepository(_context);
				}

				return _feedbacks;
			}
			private set { _feedbacks = value; }
		}

		public ICategoryRepository Categories
		{
			get
			{
				if (_categories is null)
				{
					_categories = new CategoryRepository(_context);
				}

				return _categories;
			}
			private set { _categories = value; }
		}

		public ITagRepository Tags
		{
			get
			{
				if (_tags is null)
				{
					_tags = new TagRepository(_context);
				}

				return _tags;
			}
			private set { _tags = value; }
		}

		public ICourseRequirementRepository CourseRequirements
		{
			get
			{
				if (_courseRequirements is null)
				{
					_courseRequirements = new CourseRequirementRepository(_context);
				}

				return _courseRequirements;
			}
			private set { _courseRequirements = value; }
		}

		public ICourseLearningOutcomeRepository CourseLearningOutcomes
		{
			get
			{
				if (_courseLearningOutcomes is null)
				{
					_courseLearningOutcomes = new CourseLearningOutcomeRepository(_context);
				}

				return _courseLearningOutcomes;
			}
			private set { _courseLearningOutcomes = value; }
		}

		public ICourseTargetAudienceRepository CourseTargetAudiences
		{
			get
			{
				if (_courseTargetAudiences is null)
				{
					_courseTargetAudiences = new CourseTargetAudienceRepository(_context);
				}

				return _courseTargetAudiences;
			}
			private set { _courseTargetAudiences = value; }
		}

		public ICourseTagRepository CourseTags
		{
			get
			{
				if (_courseTags is null)
				{
					_courseTags = new CourseTagRepository(_context);
				}

				return _courseTags;
			}
			private set { _courseTags = value; }
		}

		public ISectionRepository Sections
		{
			get
			{
				if (_sections is null)
				{
					_sections = new SectionRepository(_context);
				}

				return _sections;
			}
			private set { _sections = value; }
		}

		public ILessonRepository Lessons
		{
			get
			{
				if (_lessons is null)
				{
					_lessons = new LessonRepository(_context);
				}

				return _lessons;
			}
			private set { _lessons = value; }
		}

		public IQuestionRepository Questions
		{
			get
			{
				if (_questions is null)
				{
					_questions = new QuestionRepository(_context);
				}

				return _questions;
			}
			private set { _questions = value; }
		}

		public IOptionRepository Options
		{
			get
			{
				if (_options is null)
				{
					_options = new OptionRepository(_context);
				}

				return _options;
			}
			private set { _options = value; }
		}

		public IPaymentRepository Payments
		{
			get
			{
				if (_payments is null)
				{
					_payments = new PaymentRepository(_context);
				}
				return _payments;
			}
			private set { _payments = value; }
		}

        public ICourseReviewHistoryRepository CourseReviewHistories
        {
            get
            {
                if (_courseReviewHistoryRepository is null)
                {
                    _courseReviewHistoryRepository = new CourseReviewHistoryRepository(_context);
                }
                return _courseReviewHistoryRepository;
            }
            private set { _courseReviewHistoryRepository = value; }
        }
        public ILessonCommentRepository LessonComments
        {
            get
            {
                if (_lessonComments is null)
                {
                    _lessonComments = new LessonCommentRepository(_context);
                }
                return _lessonComments;
            }
            private set { _lessonComments = value; }
        }
        public IStudentAnswersRepository StudentAnswers
        {
            get
            {
                if (_studentAnswers is null)
                {
                    _studentAnswers = new StudentAnswersRepository(_context);
                }
                return _studentAnswers;
            }
            private set { _studentAnswers = value; }
        }

        public void Dispose()
		{
			_context.Dispose();
		}

		public async Task<int> SaveAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task<IDbContextTransaction> BeginTransactionAsync()
		{
			return await _context.Database.BeginTransactionAsync();
		}
	}
}
