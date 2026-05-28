namespace LoopLearn.Entities.Helpers.Models
{
    /// <summary>
    /// Returned by CourseValidationService.
    /// IsReady is true only when Errors is empty.
    /// </summary>
    public class CourseValidationResult
    {
        public bool IsReady => Errors.Count == 0;
        public List<string> Errors { get; } = new();

        public void AddError(string error) => Errors.Add(error);
    }
}
