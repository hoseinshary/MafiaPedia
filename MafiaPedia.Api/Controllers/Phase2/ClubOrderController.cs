using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs.Phase2.Finance;
using MafiaPedia.Api.IServices.Phase2;

namespace MafiaPedia.Api.Controllers.Phase2;

[ApiController]
[Route("api/club/order")]
[Produces("application/json")]
[Authorize(Policy = "ClubOnly")]
public class ClubOrderController : ControllerBase
{
    private readonly IClubOrderService _service;
    private readonly IClubUserService _clubUserService;

    public ClubOrderController(IClubOrderService service, IClubUserService clubUserService)
    {
        _service = service;
        _clubUserService = clubUserService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int clubId, int id)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier", "supervisor", "master");
        var result = await _service.GetByIdAsync(clubId, id);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        int clubId,
        [FromQuery] string? query,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var result = await _service.SearchAsync(clubId, query, fromDate, toDate);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int clubId, [FromBody] CreateClubOrderDto dto)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier", "supervisor", "master");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.CreateAsync(clubId, userId, dto);
        return Ok(result);
    }

    [HttpGet("by-clubplayer/{clubPlayerId:int}")]
    public async Task<IActionResult> GetByClubPlayerAndDate(int clubId, int clubPlayerId, [FromQuery] DateOnly businessDate)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var result = await _service.GetByClubPlayerAndDateAsync(clubId, clubPlayerId, businessDate);
        return Ok(result);
    }

    [HttpGet("open/{clubPlayerId:int}")]
    public async Task<IActionResult> GetOpenOrdersForCustomer(int clubId, int clubPlayerId)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier", "supervisor", "master");
        var result = await _service.GetOpenOrdersForCustomerAsync(clubId, clubPlayerId);
        return Ok(result);
    }

    [HttpPost("add-item")]
    public async Task<IActionResult> AddItem([FromQuery] int clubId, [FromBody] AddItemRequestDto dto)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier", "supervisor", "master");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.AddItemAsync(clubId, userId, dto.OrderId, dto.ClubPlayerId, dto.ProductId, dto.Quantity, dto.ForceNewOrder);
        return Ok(result);
    }

    [HttpPut("items/{orderItemId:int}/quantity")]
    public async Task<IActionResult> UpdateItemQuantity(int clubId, int orderItemId, [FromBody] UpdateItemQuantityRequestDto dto)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier", "supervisor", "master");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.UpdateItemQuantityAsync(clubId, userId, orderItemId, dto.NewQuantity);
        return Ok(result);
    }

    [HttpDelete("items/{orderItemId:int}")]
    public async Task<IActionResult> RemoveItem(int clubId, int orderItemId)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier", "supervisor", "master");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.RemoveItemAsync(clubId, userId, orderItemId);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int clubId)
    {
        await VerifyAccessAsync(clubId, "owner", "cashier");
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DeleteAsync(clubId, id, userId);
        return NoContent();
    }

    private async Task VerifyAccessAsync(int clubId, params string[] allowedRoles)
    {
        if (User.IsInRole("admin")) return;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var clubUser = await _clubUserService.GetClubUserAsync(userId, clubId);
        if (clubUser is null || !allowedRoles.Contains(clubUser.ClubuserRole))
            throw new Common.Exceptions.ForbiddenAppException();
    }
}