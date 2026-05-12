using LoopLearn.DataAccess.Data;
using LoopLearn.DataAccess.Implementation.Repositories;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Interfaces.Repositories;

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
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

    }
}
