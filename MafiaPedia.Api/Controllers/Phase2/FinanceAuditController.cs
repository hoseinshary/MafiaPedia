using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/club/finance-audit")]
[Produces("application/json")]
[Authorize(Policy = "ClubOnly")]
public class FinanceAuditController : ControllerBase
{
    private readonly IFinanceAuditService _service;
    private readonly IClubUserService _clubUserService;

    public FinanceAuditController(IFinanceAuditService service, IClubUserService clubUserService)
    {
        _service = service;
        _clubUserService = clubUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeletedRecords([FromQuery] int clubId, [FromQuery] string type)
    {
        await VerifyOwnerAccessAsync(clubId);
        var result = await _service.GetDeletedRecordsAsync(clubId, type);
        return Ok(result);
    }

    private async Task VerifyOwnerAccessAsync(int clubId)
    {
        if (User.IsInRole("admin")) return;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var clubUser = await _clubUserService.GetClubUserAsync(userId, clubId);
        if (clubUser is null || clubUser.ClubuserRole != "owner")
            throw new Common.Exceptions.ForbiddenAppException("فقط مالک کافه می‌تواند گزارش حذف‌شدگان را مشاهده کند");
    }
}
