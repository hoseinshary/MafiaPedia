using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;
using MafiaPedia.Api.Services.Iservices;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlaysController : ControllerBase
{
    private readonly IPlayWriteService _playWriteService;
    private readonly IPlayReadService _playReadService;

    public PlaysController(IPlayWriteService playWriteService, IPlayReadService playReadService)
    {
        _playWriteService = playWriteService;
        _playReadService = playReadService;
    }

    [HttpGet("{playId:int}")]
    [ProducesResponseType(typeof(PlayDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlay(int playId)
    {
        var play = await _playReadService.GetPlayByIdAsync(playId);
        if (play is null) return NotFound(new { message = "بازی یافت نشد" });
        return Ok(play);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlayListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlays(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _playReadService.GetPlaysAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> AddPlay([FromBody] CreatePlayDto dto)
    {
        if (dto.Players.Count == 0)
            return BadRequest("At least one player is required.");

        dto.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var playId = await _playWriteService.AddPlayAsync(dto);
        return CreatedAtAction(nameof(AddPlay), new { id = playId }, playId);
    }

    [HttpDelete("{playId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlay(int playId)
    {
        var (success, error) = await _playWriteService.DeletePlayAsync(playId);

        if (!success)
            return NotFound(new { message = error });

        return Ok(new { message = "بازی با موفقیت حذف شد" });
    }

    [HttpPut("{playId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlay(int playId, [FromBody] UpdatePlayDto dto)
    {
        var result = await _playWriteService.UpdatePlayAsync(playId, dto);
        if (!result) return NotFound(new { message = "بازی یافت نشد" });
        return Ok(new { message = "بازی با موفقیت ویرایش شد" });
    }
}
