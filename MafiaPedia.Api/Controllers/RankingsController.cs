using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;
using MafiaPedia.Api.Services.Iservices;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RankingsController : ControllerBase
{
    private readonly IRankingService _rankingService;

    public RankingsController(IRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    [HttpGet("overall")]
    [ProducesResponseType(typeof(IEnumerable<RankingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RankingDto>>> GetOverall(
        [FromQuery] OverallRankingFilterDto filter)
    {
        var rankings = await _rankingService.GetOverallRankingAsync(filter);
        return Ok(rankings);
    }

    [HttpGet("side")]
    [ProducesResponseType(typeof(IEnumerable<SideRankingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SideRankingDto>>> GetSide(
        [FromQuery] SideRankingFilterDto filter)
    {
        var rankings = await _rankingService.GetSideRankingsAsync(filter);
        return Ok(rankings);
    }
}
