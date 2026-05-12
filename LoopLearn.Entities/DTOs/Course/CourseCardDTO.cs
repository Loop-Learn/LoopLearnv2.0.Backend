namespace LoopLearn.Entities.DTOs.Course
{
    public class CourseCardDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string ThumbnailUrl { get; set; }

        public string InstructorName { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalRatings { get; set; }

        public decimal Price { get; set; }

        public bool IsFree { get; set; }

        public string Level { get; set; }

    }
}