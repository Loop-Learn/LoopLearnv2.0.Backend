namespace LoopLearn.Entities.DTOs.Learning
{
    public class CreateCommentDTO
    {
        public string Comment { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class UpdateCommentDTO
    {
        public string Comment { get; set; }
    }

    public class CommentResponseDTO
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string StudentFullName { get; set; }
        public string StudentImage { get; set; }
        
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentResponseDTO> Replies { get; set; } = new();
    }
}