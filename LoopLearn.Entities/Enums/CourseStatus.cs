namespace LoopLearn.Entities.Enums
{
    public enum CourseStatus
    {
        Draft,
        PendingReview,
        Published,
        Archived,
        Rejected,
        Submitted,   // Instructor submitted for review
        Approved,   // Admin approved → Published
    }
}
