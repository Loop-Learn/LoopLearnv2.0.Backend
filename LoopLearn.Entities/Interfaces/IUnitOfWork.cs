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
        ITagRepository Tags { get; }
        ICourseRequirementRepository CourseRequirements { get; }
        ICourseLearningOutcomeRepository CourseLearningOutcomes { get; }
        ICourseTargetAudienceRepository CourseTargetAudiences { get; }
        ICourseTagRepository CourseTags { get; }
        ISectionRepository Sections { get; }
        ILessonRepository Lessons { get; }
        IQuestionRepository Questions { get; }
        IOptionRepository Options { get; }

        Task<int> SaveAsync();
    }
}
