using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlaysController : ControllerBase
{
    private readonly IPlayWriteService _playWriteService;

    public PlaysController(IPlayWriteService playWriteService)
    {
        _playWriteService = playWriteService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> AddPlay([FromBody] CreatePlayDto dto)
    {
        if (dto.Players.Count == 0)
            return BadRequest("At least one player is required.");

        var playId = await _playWriteService.AddPlayAsync(dto);
        return CreatedAtAction(nameof(AddPlay), new { id = playId }, playId);
    }
}
