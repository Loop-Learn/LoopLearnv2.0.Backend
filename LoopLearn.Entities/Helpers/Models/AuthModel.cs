namespace LoopLearn.Entities.Helpers.Models
{
    public class AuthModel
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
