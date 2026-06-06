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

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
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
}
