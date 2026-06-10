using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;
using MafiaPedia.Api.Services.Iservices;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class DropdownController : ControllerBase
{
    private readonly IDropdownService _dropdownService;

    public DropdownController(IDropdownService dropdownService)
    {
        _dropdownService = dropdownService;
    }

    [HttpGet("clubs")]
    [ProducesResponseType(typeof(IEnumerable<DropdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DropdownDto>>> GetClubs()
    {
        return Ok(await _dropdownService.GetClubsAsync());
    }

    [HttpGet("events")]
    [ProducesResponseType(typeof(IEnumerable<EventDropdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventDropdownDto>>> GetEvents()
    {
        return Ok(await _dropdownService.GetEventsAsync());
    }

    [HttpGet("scenarios")]
    [ProducesResponseType(typeof(IEnumerable<DropdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DropdownDto>>> GetScenarios()
    {
        return Ok(await _dropdownService.GetScenariosAsync());
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(IEnumerable<RoleDropdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleDropdownDto>>> GetRoles()
    {
        return Ok(await _dropdownService.GetRolesAsync());
    }
}
