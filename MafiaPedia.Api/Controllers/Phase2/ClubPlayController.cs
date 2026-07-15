using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.ClubPlay;
using MafiaPedia.Api.DTOs.Phase2.Master;
using MafiaPedia.Api.IServices.Phase2;
using MafiaPedia.Api.Utils;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/clubs/{clubId:int}/clubplays")]
[Produces("application/json")]
public class ClubPlayController : ClubControllerBase
{
    private readonly IClubPlayService _clubPlayService;
    private readonly IMasterAuthService _masterAuthService;

    public ClubPlayController(IClubPlayService clubPlayService, IMasterAuthService masterAuthService, IClubUserService clubUserService)
        : base(masterAuthService, clubUserService)
    {
        _clubPlayService = clubPlayService;
        _masterAuthService = masterAuthService;
    }

    [HttpPost]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> CreateClubPlay(int clubId, [FromBody] CreateClubPlayDto dto)
    {
        var (masterId, err) = await ResolveTargetMasterIdAsync(clubId, dto.MasterId);
        if (err is not null) return err;

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _clubPlayService.CreateClubPlayAsync(clubId, masterId!.Value, userId, dto);
            return CreatedAtAction(nameof(GetClubPlayDetail), new { clubId, playId = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{playId:int}")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> GetClubPlayDetail(int clubId, int playId)
    {
        var forbid = await VerifyClubAccess(clubId, "master", "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        var result = await _clubPlayService.GetClubPlayDetailAsync(clubId, playId);
        if (result is null)
            return NotFound(new { message = "بازی یافت نشد" });

        return Ok(result);
    }

    [HttpPost("{playId:int}/reshuffle-roles")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> ReshuffleRoles(int clubId, int playId)
    {
        var (restrictToMasterId, err) = await ResolveOwnershipRestrictionAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.ReshuffleRolesAsync(clubId, playId, restrictToMasterId);
            if (result is null)
                return NotFound(new { message = "بازی یافت نشد" });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("count-by-date")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> GetPlayCountByDate(int clubId, [FromQuery] DateOnly date, [FromQuery] int? masterId = null)
    {
        int resolvedMasterId;

        if (masterId.HasValue && await IsPrivilegedClubStaffAsync(clubId))
        {
            resolvedMasterId = masterId.Value;
        }
        else
        {
            var (ctx, err) = await ResolveMasterAsync(clubId);
            if (err is not null) return err;
            resolvedMasterId = ctx!.MasterId;
        }

        var count = await _clubPlayService.GetPlayCountByDateAsync(clubId, resolvedMasterId, date);
        return Ok(new { count });
    }

    // ── Status transitions ──

    [HttpPost("{playId:int}/confirm-reveal")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> ConfirmRoleReveal(int clubId, int playId)
    {
        var (restrictToMasterId, err) = await ResolveOwnershipRestrictionAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.ConfirmRoleRevealAsync(clubId, playId, restrictToMasterId);
            if (result is null) return NotFound(new { message = "بازی یافت نشد" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{playId:int}/submit-winnerside")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> SubmitWinnerside(int clubId, int playId, [FromBody] SubmitWinnersideRequestDto body)
    {
        var (restrictToMasterId, err) = await ResolveOwnershipRestrictionAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.SubmitWinnersideAsync(clubId, playId, restrictToMasterId, body.WinnersideId);
            if (result is null) return NotFound(new { message = "بازی یافت نشد" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{playId:int}/submit-ranks")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> SubmitRanks(int clubId, int playId, [FromBody] List<ParticipantRankDto> ranks)
    {
        var (restrictToMasterId, err) = await ResolveOwnershipRestrictionAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.SubmitRanksAsync(clubId, playId, restrictToMasterId, ranks);
            if (result is null) return NotFound(new { message = "بازی یافت نشد" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Dashboard reads ──

    [HttpGet("by-date")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> GetPlaysByBusinessDate(int clubId, [FromQuery] DateOnly? date)
    {
        var (ctx, err) = await ResolveMasterAsync(clubId);
        if (err is not null) return err;

        var businessDate = date ?? BusinessDateHelper.Today();
        var result = await _clubPlayService.GetPlaysByBusinessDateAsync(clubId, ctx!.MasterId, businessDate);
        return Ok(result);
    }

    [HttpGet("club-by-date")]
    [Authorize(Policy = "AdminOrClub")]
    public async Task<IActionResult> GetClubPlaysByBusinessDate(int clubId, [FromQuery] DateOnly? date)
    {
        var forbid = await VerifyClubAccess(clubId, "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        var businessDate = date ?? BusinessDateHelper.Today();
        var result = await _clubPlayService.GetClubPlaysByBusinessDateAsync(clubId, businessDate);
        return Ok(result);
    }

    [HttpGet("open")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> GetOpenPlays(int clubId)
    {
        var (ctx, err) = await ResolveMasterAsync(clubId);
        if (err is not null) return err;

        var result = await _clubPlayService.GetOpenPlaysAsync(clubId, ctx!.MasterId);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> GetMyPlays(
        int clubId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? status = null)
    {
        var (ctx, err) = await ResolveMasterAsync(clubId);
        if (err is not null) return err;

        var (items, total) = await _clubPlayService.GetMyPlaysAsync(clubId, ctx!.MasterId, page, pageSize, dateFrom, dateTo, status);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("club-stats")]
    [Authorize(Policy = "AdminOrClub")]
    public async Task<IActionResult> GetClubStats(int clubId, [FromQuery] string period)
    {
        var forbid = await VerifyClubAccess(clubId, "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        try
        {
            return Ok(await _clubPlayService.GetClubStatsAsync(clubId, period));
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("master-performance")]
    [Authorize(Policy = "AdminOrClub")]
    public async Task<IActionResult> GetMasterPerformance(int clubId, [FromQuery] string period)
    {
        var forbid = await VerifyClubAccess(clubId, "owner");
        if (forbid is not null) return forbid;

        try
        {
            return Ok(await _clubPlayService.GetMasterPerformanceAsync(clubId, period));
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("my-stats")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> GetMyStats(int clubId, [FromQuery] string period = "week")
    {
        var (ctx, err) = await ResolveMasterAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.GetMyStatsAsync(clubId, ctx!.MasterId, period);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Editing ──

    [HttpPut("{playId:int}")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> UpdateClubPlay(int clubId, int playId, [FromBody] UpdateClubPlayDto dto)
    {
        var (restrictToMasterId, err) = await ResolveOwnershipRestrictionAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.UpdateClubPlayAsync(clubId, playId, restrictToMasterId, dto);
            if (result is null) return NotFound(new { message = "بازی یافت نشد" });
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{playId:int}/participants/{clubPlayerId:int}")]
    [Authorize(Policy = "ClubOnly")]
    public async Task<IActionResult> ReplaceParticipant(int clubId, int playId, int clubPlayerId, [FromBody] ReplaceParticipantDto dto)
    {
        var (restrictToMasterId, err) = await ResolveOwnershipRestrictionAsync(clubId);
        if (err is not null) return err;

        try
        {
            var result = await _clubPlayService.ReplaceParticipantAsync(clubId, playId, restrictToMasterId, clubPlayerId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Helpers ──

    private async Task<(MasterContextDto? Context, IActionResult? Error)> ResolveMasterAsync(int clubId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var masterContext = await _masterAuthService.GetMasterContextByUserIdAsync(userId);

        if (masterContext is null)
            return (null, Forbid());

        if (masterContext.ClubId != clubId)
            return (null, StatusCode(403, new { message = "شما به این کافه دسترسی ندارید" }));

        return (masterContext, null);
    }

    private async Task<(int? MasterId, IActionResult? Error)> ResolveTargetMasterIdAsync(int clubId, int? requestedMasterId)
    {
        var isPrivileged = await IsPrivilegedClubStaffAsync(clubId);

        if (isPrivileged || User.IsInRole("admin"))
        {
            if (requestedMasterId is null)
                return (null, BadRequest(new { message = "انتخاب گرداننده الزامی است" }));
            return (requestedMasterId, null);
        }

        var (ctx, err) = await ResolveMasterAsync(clubId);
        if (err is not null) return (null, err);
        return (ctx!.MasterId, null);
    }

    private async Task<(int? RestrictToMasterId, IActionResult? Error)> ResolveOwnershipRestrictionAsync(int clubId)
    {
        if (await IsPrivilegedClubStaffAsync(clubId) || User.IsInRole("admin"))
            return (null, null);

        var (ctx, err) = await ResolveMasterAsync(clubId);
        if (err is not null) return (null, err);
        return (ctx!.MasterId, null);
    }

    private async Task<bool> IsPrivilegedClubStaffAsync(int clubId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var clubUser = await ClubUserService.GetClubUserAsync(userId, clubId);
        return clubUser is not null &&
            (clubUser.ClubuserRole == "owner" || clubUser.ClubuserRole == "supervisor" || clubUser.ClubuserRole == "cashier");
    }
}
