using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.Club;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/clubs/{clubId:int}/members")]
[Produces("application/json")]
[Authorize(Policy = "AdminOrClub")]
public class ClubUserController : ClubControllerBase
{
    private readonly IClubUserService _service;

    public ClubUserController(IClubUserService service, IMasterAuthService masterAuthService)
        : base(masterAuthService, service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(int clubId)
    {
        var forbid = await VerifyClubAccess(clubId, "owner", "supervisor", "cashier", "master");
        if (forbid is not null) return forbid;

        return Ok(await _service.GetMembersAsync(clubId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember(int clubId, [FromBody] CreateClubUserDto dto)
    {
        var isAdmin = User.IsInRole("admin");

        if (dto.ClubuserRole == "owner")
        {
            if (!isAdmin) return Forbid();
        }
        else
        {
            var forbid = await VerifyClubAccess(clubId, "owner");
            if (forbid is not null) return forbid;
        }

        try
        {
            var result = await _service.CreateMemberAsync(clubId, dto, isAdmin);
            return CreatedAtAction(nameof(GetMembers), new { clubId }, result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPut("{memberId:int}")]
    public async Task<IActionResult> UpdateMemberRole(int clubId, int memberId, [FromBody] UpdateClubUserRoleDto dto)
    {
        var isAdmin = User.IsInRole("admin");

        var forbid = await VerifyClubAccess(clubId, "owner");
        if (forbid is not null) return forbid;

        if (dto.ClubuserRole == "owner" && !isAdmin) return Forbid();

        try
        {
            var result = await _service.UpdateMemberRoleAsync(clubId, memberId, dto, isAdmin);
            if (result is null) return NotFound(new { message = "عضو یافت نشد" });
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpDelete("{memberId:int}")]
    public async Task<IActionResult> DeleteMember(int clubId, int memberId)
    {
        var forbid = await VerifyClubAccess(clubId, "owner");
        if (forbid is not null) return forbid;

        var deleted = await _service.DeleteMemberAsync(clubId, memberId);
        if (!deleted) return NotFound(new { message = "عضو یافت نشد" });
        return Ok(new { message = "عضو با موفقیت حذف شد" });
    }
}
