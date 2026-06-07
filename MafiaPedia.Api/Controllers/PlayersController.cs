using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IPlayerCommentService _playerCommentService;

    public PlayersController(IPlayerService playerService, IPlayerCommentService playerCommentService)
    {
        _playerService = playerService;
        _playerCommentService = playerCommentService;
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
}
