namespace LoopLearn.Entities.Helpers.Models
{
    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double Duration { get; set; }
    }
}
