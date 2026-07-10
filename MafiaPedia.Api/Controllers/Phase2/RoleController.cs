using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.Data;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/roles")]
[Produces("application/json")]
public class RoleController : ControllerBase
{
    private readonly MafiaDbContext _context;
    private readonly IWebHostEnvironment _env;

    public RoleController(MafiaDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPut("{roleId:int}/photo")]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadRolePhoto(int roleId, IFormFile photo)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role is null)
            return NotFound(new { message = "نقش یافت نشد" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "فرمت فایل مجاز نیست. فقط jpg, jpeg, png, webp" });

        if (photo.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "حجم فایل باید کمتر از ۲ مگابایت باشد" });

        if (!string.IsNullOrEmpty(role.Photo))
        {
            var oldPath = Path.Combine(_env.WebRootPath, role.Photo.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "roles");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/roles/{fileName}";
        role.Photo = relativePath;
        await _context.SaveChangesAsync();

        return Ok(new { photo = relativePath });
    }
}
