using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

public abstract class ClubControllerBase : ControllerBase
{
    protected readonly IMasterAuthService MasterAuthService;

    protected ClubControllerBase(IMasterAuthService masterAuthService)
    {
        MasterAuthService = masterAuthService;
    }

    protected async Task<IActionResult?> VerifyMasterClubAccess(int clubId)
    {
        if (!User.IsInRole("master") || User.IsInRole("admin"))
            return null;

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var masterContext = await MasterAuthService.GetMasterContextByUserIdAsync(userId);
        if (masterContext is null || masterContext.ClubId != clubId)
            return Forbid();

        return null;
    }
}
