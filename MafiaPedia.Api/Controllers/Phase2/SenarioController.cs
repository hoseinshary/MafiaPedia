using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Senario;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Produces("application/json")]
public class SenarioController : ControllerBase
{
    private readonly IDropdownService _dropdownService;

    public SenarioController(IDropdownService dropdownService)
    {
        _dropdownService = dropdownService;
    }

    [HttpGet("api/senarios/{senarioId:int}/role-set")]
    [Authorize(Policy = "AdminOrClub")]
    public async Task<IActionResult> GetRoleSet(int senarioId, [FromQuery] int playerCount)
    {
        var result = await _dropdownService.GetRoleSetAsync(senarioId, playerCount);
        if (result is null)
            return NotFound(new { message = "این سناریو برای این تعداد بازیکن پیکربندی نشده است" });

        return Ok(result);
    }
}