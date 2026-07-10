using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.Club;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/clubs")]
[Produces("application/json")]
public class ClubManagementController : ClubControllerBase
{
    private readonly IClubManagementService _service;

    public ClubManagementController(IClubManagementService service, IMasterAuthService masterAuthService)
        : base(masterAuthService)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllClubs()
    {
        var clubs = await _service.GetAllClubsAsync();
        return Ok(clubs);
    }

    [Authorize(Policy = "AdminOrMaster")]
    [HttpGet("{clubId:int}")]
    public async Task<IActionResult> GetClubDetail(int clubId)
    {
        var forbid = await VerifyMasterClubAccess(clubId);
        if (forbid is not null) return forbid;

        var club = await _service.GetClubDetailAsync(clubId);
        if (club is null)
            return NotFound(new { message = "باشگاه یافت نشد" });

        return Ok(club);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateClub([FromForm] CreateClubDto dto, IFormFile? logo)
    {
        var club = await _service.CreateClubAsync(dto);
        if (logo != null)
            await _service.SaveLogoAsync(club.Id, logo);
        return CreatedAtAction(nameof(GetClubDetail), new { clubId = club.Id }, club);
    }

    [HttpPut("{clubId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateClub(int clubId, [FromForm] UpdateClubDto dto, IFormFile? logo)
    {
        if (logo != null)
            await _service.SaveLogoAsync(clubId, logo);

        var result = await _service.UpdateClubAsync(clubId, dto);
        if (result is null)
            return NotFound(new { message = "باشگاه یافت نشد" });

        return Ok(result);
    }

    [HttpDelete("{clubId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteClub(int clubId)
    {
        try
        {
            var deleted = await _service.DeleteClubAsync(clubId);
            if (!deleted)
                return NotFound(new { message = "باشگاه یافت نشد" });

            return Ok(new { message = "باشگاه با موفقیت حذف شد" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{clubId:int}/rooms")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateRoom(int clubId, [FromBody] CreateRoomDto dto)
    {
        try
        {
            var room = await _service.CreateRoomAsync(clubId, dto);
            return CreatedAtAction(nameof(GetClubDetail), new { clubId }, room);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "باشگاه یافت نشد" });
        }
    }

    [HttpPut("{clubId:int}/rooms/{roomId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateRoom(int clubId, int roomId, [FromBody] UpdateRoomDto dto)
    {
        var result = await _service.UpdateRoomAsync(roomId, dto);
        if (result is null)
            return NotFound(new { message = "اتاق یافت نشد" });

        return Ok(result);
    }

    [HttpDelete("{clubId:int}/rooms/{roomId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteRoom(int clubId, int roomId)
    {
        try
        {
            var deleted = await _service.DeleteRoomAsync(roomId);
            if (!deleted)
                return NotFound(new { message = "اتاق یافت نشد" });

            return Ok(new { message = "اتاق با موفقیت حذف شد" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{clubId:int}/masters")]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMaster(int clubId, [FromForm] CreateMasterDto dto, IFormFile? photo)
    {
        try
        {
            var master = await _service.CreateMasterAsync(clubId, dto);
            if (photo != null)
                await _service.SaveMasterPhotoAsync(master.Id, photo);
            return CreatedAtAction(nameof(GetClubDetail), new { clubId }, master);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "باشگاه یافت نشد" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{clubId:int}/masters/{masterId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMaster(int clubId, int masterId, [FromForm] UpdateMasterDto dto, IFormFile? photo)
    {
        try
        {
            if (photo != null)
                await _service.SaveMasterPhotoAsync(masterId, photo);

            var result = await _service.UpdateMasterAsync(masterId, dto);
            if (result is null)
                return NotFound(new { message = "گرداننده یافت نشد" });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{clubId:int}/masters/{masterId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteMaster(int clubId, int masterId)
    {
        try
        {
            var deleted = await _service.DeleteMasterAsync(masterId);
            if (!deleted)
                return NotFound(new { message = "گرداننده یافت نشد" });

            return Ok(new { message = "گرداننده با موفقیت حذف شد" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
