using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/masters")]
[Produces("application/json")]
[Authorize]
public class MasterAuthController : ControllerBase
{
    private readonly IMasterAuthService _masterAuthService;

    public MasterAuthController(IMasterAuthService masterAuthService)
    {
        _masterAuthService = masterAuthService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyMasterContext()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var context = await _masterAuthService.GetMasterContextByUserIdAsync(userId);

        if (context is null)
            return NotFound(new { message = "کاربر به هیچ گرداننده‌ای متصل نیست" });

        return Ok(context);
    }
}
