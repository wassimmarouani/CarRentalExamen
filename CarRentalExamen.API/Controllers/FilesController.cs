using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalExamen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private readonly IWebHostEnvironment _env;

    public FilesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("car-image")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<CarImageUploadResponse>> UploadCarImage([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest("File is too large. Max 5MB allowed.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return BadRequest("Unsupported file type. Use jpg, jpeg, png, or webp.");
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "cars");
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var relativeUrl = $"/uploads/cars/{fileName}";

        return Ok(new CarImageUploadResponse
        {
            Url = $"{baseUrl}{relativeUrl}",
            RelativeUrl = relativeUrl
        });
    }
}

public class CarImageUploadResponse
{
    public string Url { get; set; } = string.Empty;
    public string RelativeUrl { get; set; } = string.Empty;
}
