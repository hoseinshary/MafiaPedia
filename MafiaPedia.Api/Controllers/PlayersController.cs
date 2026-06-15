using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;
using MafiaPedia.Api.Services.Iservices;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IPlayerCommentService _playerCommentService;
    private readonly IPlayReadService _playReadService;

    public PlayersController(IPlayerService playerService, IPlayerCommentService playerCommentService, IPlayReadService playReadService)
    {
        _playerService = playerService;
        _playerCommentService = playerCommentService;
        _playReadService = playReadService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlayerListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _playerService.GetPlayersAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{playerId:int}")]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerProfileDto>> GetProfile(int playerId)
    {
        var profile = await _playerService.GetProfileAsync(playerId);
        if (profile is null) return NotFound();
        return Ok(profile);
    }

    [HttpGet("{playerId:int}/detail")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PlayerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerDetail(int playerId)
    {
        var player = await _playerService.GetPlayerDetailAsync(playerId);
        if (player is null) return NotFound(new { message = "بازیکن یافت نشد" });
        return Ok(player);
    }

    [HttpGet("{playerId:int}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int playerId)
    {
        var comments = await _playerCommentService.GetCommentsAsync(playerId);
        return Ok(comments);
    }

    [HttpPost("{playerId:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommentDto>> AddComment(int playerId, [FromBody] CreateCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Content is required.");

        var comment = await _playerCommentService.AddCommentAsync(playerId, dto);
        return CreatedAtAction(nameof(GetComments), new { playerId }, comment);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePlayer([FromForm] CreatePlayerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        try
        {
            var player = await _playerService.CreatePlayerAsync(dto);
            return CreatedAtAction(nameof(GetProfile), new { playerId = player.Id }, player);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{playerId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeletePlayer(int playerId)
    {
        var (success, error) = await _playerService.DeletePlayerAsync(playerId);

        if (!success && error == "بازیکن یافت نشد")
            return NotFound(new { message = error });

        if (!success)
            return Conflict(new { message = error });

        return Ok(new { message = "بازیکن با موفقیت حذف شد" });
    }

    [HttpPut("{playerId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlayer(int playerId, [FromForm] UpdatePlayerDto dto)
    {
        try
        {
            var result = await _playerService.UpdatePlayerAsync(playerId, dto);
            if (!result) return NotFound(new { message = "بازیکن یافت نشد" });
            return Ok(new { message = "بازیکن با موفقیت ویرایش شد" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<PlayerSearchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PlayerSearchDto>>> SearchPlayers(
        [FromQuery] string query,
        [FromQuery] int limit = 10)
    {
        var trimmed = query?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < 2)
            return Ok(Enumerable.Empty<PlayerSearchDto>());

        if (limit < 1) limit = 1;
        if (limit > 100) limit = 100;

        var results = await _playerService.SearchPlayersAsync(trimmed, limit);
        return Ok(results);
    }

    [HttpGet("head-to-head")]
    [ProducesResponseType(typeof(HeadToHeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHeadToHead([FromQuery] int player1Id, [FromQuery] int player2Id)
    {
        if (player1Id <= 0 || player2Id <= 0 || player1Id == player2Id)
            return BadRequest(new { message = "Both player1Id and player2Id are required and must be different." });

        var result = await _playReadService.GetHeadToHeadAsync(player1Id, player2Id);
        if (result is null) return NotFound(new { message = "One or both players not found." });

        return Ok(result);
    }
}
