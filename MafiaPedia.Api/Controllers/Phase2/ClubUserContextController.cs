using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/clubusers")]
[Produces("application/json")]
[Authorize]
public class ClubUserContextController : ControllerBase
{
    private readonly IClubUserService _service;

    public ClubUserContextController(IClubUserService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyClubs()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.GetMyClubsAsync(userId);
        return Ok(result);
    }
}
