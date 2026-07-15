using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.Event;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/clubs/{clubId:int}/events")]
[Produces("application/json")]
public class EventController : ClubControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService, IMasterAuthService masterAuthService, IClubUserService clubUserService)
        : base(masterAuthService, clubUserService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOrClub")]
    public async Task<IActionResult> GetClubEvents(int clubId)
    {
        var forbid = await VerifyClubAccess(clubId, "master", "owner", "supervisor", "cashier");
        if (forbid is not null) return forbid;

        try
        {
            var events = await _eventService.GetClubEventsAsync(clubId);
            return Ok(events);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateEvent(int clubId, [FromBody] CreateEventDto dto)
    {
        try
        {
            var result = await _eventService.CreateEventAsync(clubId, dto);
            return CreatedAtAction(nameof(GetClubEvents), new { clubId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{eventId:int}/set-default")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetDefaultEvent(int clubId, int eventId)
    {
        try
        {
            var result = await _eventService.SetDefaultEventAsync(clubId, eventId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
