using Microsoft.AspNetCore.Http;

namespace LoopLearn.Entities.Helpers.Models
{
    public class UploadRequest
    {
        public IFormFile File { get; set; }
        public string Type { get; set; }
    }
}
