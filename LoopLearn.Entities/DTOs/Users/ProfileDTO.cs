using LoopLearn.Entities.Enums;

namespace LoopLearn.Entities.DTOs.Users
{
    public class ProfileDTO
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
        public bool IsVerifiedEmail { get; set; }
        public bool IsVerifiedPhone { get; set; }
        public DateTime JoinDate { get; set; }
        public string Avatar { get; set; }
    }
}
