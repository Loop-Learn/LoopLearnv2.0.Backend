using LoopLearn.Entities.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile([FromForm] UploadRequest model)
    {
        var file = model.File;
        var type = model.Type?.ToLowerInvariant();

        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        // Define allowed types and their configurations
        var allowedTypes = new Dictionary<string, (string[] Extensions, long MaxSize, string Folder)>
        {
            ["avatar"] = (new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }, 2 * 1024 * 1024, "avatars"),
            ["course-thumbnail"] = (new[] { ".jpg", ".jpeg", ".png", ".webp" }, 5 * 1024 * 1024, "course-thumbnails")
        };

        if (!allowedTypes.ContainsKey(type))
            return BadRequest(new { success = false, message = "Invalid upload type. Allowed: avatar, course-thumbnail" });

        var config = allowedTypes[type];
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!config.Extensions.Contains(extension))
            return BadRequest(new { success = false, message = $"Invalid file type for {type}. Allowed: {string.Join(", ", config.Extensions)}" });

        if (file.Length > config.MaxSize)
            return BadRequest(new { success = false, message = $"File size exceeds limit ({config.MaxSize / 1024 / 1024} MB)." });

        // Build folder path: wwwroot/uploads/{Folder}
        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", config.Folder);
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Build public URL (adjust base URL if using a proxy or CDN)
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var fileUrl = $"{baseUrl}/uploads/{config.Folder}/{uniqueFileName}";

        return Ok(new { success = true, url = fileUrl });
    }
}