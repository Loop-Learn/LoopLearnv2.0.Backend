using LoopLearn.Entities.Interfaces.Repositories;

namespace LoopLearn.Entities.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IEnrollmentRepository Enrollments { get; }
        ILessonProgressRepository LessonProgresses { get; }
        IQuizRepository Quizzes { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IFeedbackRepository Feedbacks { get; }
        ICategoryRepository Categories { get; }

        Task<int> SaveAsync();
    }
}
