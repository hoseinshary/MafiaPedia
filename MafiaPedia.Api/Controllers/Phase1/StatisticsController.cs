using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Controllers.Phase1;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(StatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics([FromQuery] StatisticsFilterDto filter)
    {
        var result = await _statisticsService.GetStatisticsAsync(filter);
        return Ok(result);
    }

    [HttpGet("home")]
    [ProducesResponseType(typeof(StatisticsHomeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsHome(  )
    {
        var result = await _statisticsService.GetStatisticsHomeAsync();
        return Ok(result);
    }
}