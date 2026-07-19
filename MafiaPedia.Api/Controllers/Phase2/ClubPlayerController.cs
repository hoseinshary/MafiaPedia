using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.ClubPlayer;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Produces("application/json")]
public class ClubPlayerController : ClubControllerBase
{
    private readonly IClubPlayerService _service;
    private readonly IWebHostEnvironment _env;

    public ClubPlayerController(IClubPlayerService service, IWebHostEnvironment env, IMasterAuthService masterAuthService, IClubUserService clubUserService)
        : base(masterAuthService, clubUserService)
    {
        _service = service;
        _env = env;
    }

    [Authorize(Policy = "AdminOrClub")]
    [HttpGet("api/clubs/{clubId:int}/customers")]
    public async Task<IActionResult> GetClubPlayers(int clubId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var forbid = await VerifyClubAccess(clubId, "master", "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        var (items, total) = await _service.GetClubPlayersAsync(clubId, page, pageSize, search);
        return Ok(new
        {
            items,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    [Authorize(Policy = "AdminOrClub")]
    [HttpGet("api/clubs/{clubId:int}/customers/{customerId:int}")]
    public async Task<IActionResult> GetClubPlayerDetail(int clubId, int customerId)
    {
        var forbid = await VerifyClubAccess(clubId, "master", "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        var result = await _service.GetClubPlayerDetailAsync(clubId, customerId);
        if (result is null)
            return NotFound(new { message = "مشتری یافت نشد" });

        return Ok(result);
    }

    [Authorize(Policy = "AdminOrClub")]
    [HttpPost("api/clubs/{clubId:int}/customers")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateOrJoin(int clubId, [FromForm] CreateOrJoinClubPlayerDto dto, IFormFile? picture)
    {
        var forbid = await VerifyClubAccess(clubId, "master", "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        string? picturePath = null;
        if (picture is not null)
            picturePath = await SaveCustomerPictureAsync(picture);

        var result = await _service.CreateOrJoinAsync(clubId, dto, picturePath);
        return StatusCode(201, result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("api/clubs/{clubId:int}/customers/{customerId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateClubPlayer(int clubId, int customerId, [FromForm] UpdateClubPlayerDto dto, IFormFile? picture)
    {
        string? newPicturePath = null;
        if (picture is not null)
            newPicturePath = await SaveCustomerPictureAsync(picture);

        var result = await _service.UpdateClubPlayerAsync(customerId, dto, newPicturePath);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("api/clubs/{clubId:int}/customers/{customerId:int}")]
    public async Task<IActionResult> RemoveFromClub(int clubId, int customerId)
    {
        await _service.RemoveFromClubAsync(clubId, customerId);
        return NoContent();
    }

    [Authorize(Policy = "AdminOrClub")]
    [HttpGet("api/clubs/{clubId:int}/customers/search-all")]
    public async Task<IActionResult> SearchAll(int clubId, [FromQuery] string query)
    {
        var forbid = await VerifyClubAccess(clubId, "master", "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        var result = await _service.SearchAllAsync(clubId, query);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOrClub")]
    [HttpGet("api/customers/search-by-mobile")]
    public async Task<IActionResult> SearchByMobile([FromQuery] string mobile)
    {
        var result = await _service.SearchByMobileAsync(mobile);
        if (result is null)
            return NotFound(new { message = "مشتری با این شماره یافت نشد" });

        return Ok(result);
    }

    private async Task<string> SaveCustomerPictureAsync(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("فرمت فایل مجاز نیست. فقط jpg, jpeg, png, webp");

        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("حجم فایل باید کمتر از ۲ مگابایت باشد");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "customers");

        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/customers/{fileName}";
    }
}
