namespace LoopLearn.Entities.Enums
{
    public enum CourseReviewAction
    {
        Submitted,   // Instructor submitted for review
        Approved,    // Admin approved → Published
        Rejected,    // Admin rejected → back to Rejected
        Archived     // Published → Archived
    }
}
