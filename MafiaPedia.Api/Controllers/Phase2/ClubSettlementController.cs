using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/club/settlement")]
[Produces("application/json")]
[Authorize(Policy = "ClubOnly")]
public class ClubSettlementController : ControllerBase
{
    private readonly IClubSettlementService _service;
    private readonly IClubUserService _clubUserService;

    public ClubSettlementController(IClubSettlementService service, IClubUserService clubUserService)
    {
        _service = service;
        _clubUserService = clubUserService;
    }

    [HttpGet("balance/{clubPlayerId:int}")]
    public async Task<IActionResult> GetBalance(int clubPlayerId, [FromQuery] int clubId, [FromQuery] DateOnly? businessDate = null)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var result = await _service.GetBalanceAsync(clubId, clubPlayerId, businessDate);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int clubId, [FromBody] CreateSettlementDto dto)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.CreateAsync(clubId, userId, dto);
        return CreatedAtAction(nameof(GetBalance), new { clubPlayerId = dto.ClubPlayerId, clubId }, result);
    }

    [HttpGet("ledger/{clubPlayerId:int}")]
    public async Task<IActionResult> GetLedger(int clubPlayerId, [FromQuery] int clubId)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var result = await _service.GetLedgerAsync(clubId, clubPlayerId);
        return Ok(result);
    }

    [HttpGet("today-overview")]
    public async Task<IActionResult> GetTodayOverview([FromQuery] int clubId, [FromQuery] string status = "all", [FromQuery] string? date = null)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var result = await _service.GetTodayOverviewAsync(clubId, status, date);
        return Ok(result);
    }

    [HttpGet("debtors")]
    public async Task<IActionResult> GetDebtors([FromQuery] int clubId)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var result = await _service.GetDebtorsAsync(clubId);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int clubId)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeleteAsync(clubId, id, userId);
        return NoContent();
    }

    private async Task VerifyAccessAsync(int clubId, params string[] allowedRoles)
    {
        if (User.IsInRole("admin")) return;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var clubUser = await _clubUserService.GetClubUserAsync(userId, clubId);
        if (clubUser is null || !allowedRoles.Contains(clubUser.ClubuserRole))
            throw new Common.Exceptions.ForbiddenAppException();
    }
}