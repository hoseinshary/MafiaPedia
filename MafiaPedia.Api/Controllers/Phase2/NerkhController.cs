using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/club/nerkh")]
[Produces("application/json")]
[Authorize(Policy = "ClubOnly")]
public class NerkhController : ControllerBase
{
    private readonly INerkhService _service;
    private readonly IClubUserService _clubUserService;

    public NerkhController(INerkhService service, IClubUserService clubUserService)
    {
        _service = service;
        _clubUserService = clubUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int clubId)
    {
        return Ok(await _service.GetAllAsync(clubId));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int clubId, [FromBody] CreateNerkhDto dto)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.CreateAsync(clubId, userId, dto);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromQuery] int clubId, [FromBody] UpdateNerkhDto dto)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.UpdateAsync(clubId, id, userId, dto);
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
