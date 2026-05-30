using LoopLearn.Entities.Enums;

namespace LoopLearn.DataAccess.Services.Course
{
    public class CourseWorkflowService
    {
        // Allowed transitions: from → to
        private static readonly Dictionary<CourseStatus, HashSet<CourseStatus>> _allowedTransitions = new()
        {
            [CourseStatus.Draft] = new() { CourseStatus.PendingReview },
            [CourseStatus.Rejected] = new() { CourseStatus.PendingReview },
            [CourseStatus.PendingReview] = new() { CourseStatus.Published, CourseStatus.Rejected },
            [CourseStatus.Published] = new() { CourseStatus.Archived },
            [CourseStatus.Archived] = new() { }   // terminal
        };

        public static bool CanTransition(CourseStatus from, CourseStatus to) =>
            _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

        public static bool CanSubmitForReview(CourseStatus status) =>
            CanTransition(status, CourseStatus.PendingReview);

        public static bool CanApprove(CourseStatus status) =>
            CanTransition(status, CourseStatus.Published);

        public static bool CanReject(CourseStatus status) =>
            CanTransition(status, CourseStatus.Rejected);

        public static bool CanArchive(CourseStatus status) =>
            CanTransition(status, CourseStatus.Archived);
    }
}
