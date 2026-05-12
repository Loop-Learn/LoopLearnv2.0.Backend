namespace LoopLearn.Entities.DTOs.Course
{
    public class CourseFilterDTO
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "Id"; //Id,Price, Rating

        public bool SortDescending { get; set; } = true;
    }
}