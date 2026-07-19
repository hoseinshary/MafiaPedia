using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.Services.Phase1;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Controllers.Phase1;

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
    public async Task<IActionResult> GetPlays([FromQuery] PlayFilterDto filter)
    {
        var result = await _playReadService.GetPlaysAsync(filter);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> AddPlay([FromForm] CreatePlayDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PlayersJson))
            return BadRequest("At least one player is required.");

        dto.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var playId = await _playWriteService.AddPlayAsync(dto);
            return CreatedAtAction(nameof(AddPlay), new { id = playId }, playId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlay(int playId, [FromForm] UpdatePlayDto dto)
    {
        try
        {
            var result = await _playWriteService.UpdatePlayAsync(playId, dto);
            if (!result) return NotFound(new { message = "بازی یافت نشد" });
            return Ok(new { message = "بازی با موفقیت ویرایش شد" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
