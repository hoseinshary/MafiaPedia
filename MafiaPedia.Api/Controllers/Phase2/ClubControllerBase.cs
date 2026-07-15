using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

public abstract class ClubControllerBase : ControllerBase
{
    protected readonly IMasterAuthService MasterAuthService;
    protected readonly IClubUserService ClubUserService;

    protected ClubControllerBase(IMasterAuthService masterAuthService, IClubUserService clubUserService)
    {
        MasterAuthService = masterAuthService;
        ClubUserService = clubUserService;
    }

    protected async Task<IActionResult?> VerifyClubAccess(int clubId, params string[] allowedRoles)
    {
        if (User.IsInRole("admin")) return null;
        if (!User.IsInRole("club")) return Forbid();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var clubUser = await ClubUserService.GetClubUserAsync(userId, clubId);
        if (clubUser is null || !allowedRoles.Contains(clubUser.ClubuserRole))
            return Forbid();

        return null;
    }

    protected Task<IActionResult?> VerifyMasterClubAccess(int clubId) => VerifyClubAccess(clubId, "master");
}
