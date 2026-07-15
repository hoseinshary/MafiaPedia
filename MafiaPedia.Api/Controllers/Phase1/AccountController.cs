using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Controllers.Phase1;

[ApiController]
[Route("api/account")]
[Produces("application/json")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAccount()
    {
        var userId = GetUserId();
        try
        {
            return Ok(await _accountService.GetMyAccountAsync(userId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountDto dto)
    {
        var userId = GetUserId();
        try
        {
            return Ok(await _accountService.UpdateAccountAsync(userId, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        try
        {
            await _accountService.ChangePasswordAsync(userId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("me/picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateLinkedPicture([FromForm] string target, [FromForm] IFormFile file)
    {
        var userId = GetUserId();
        try
        {
            var path = await _accountService.UpdateLinkedPictureAsync(userId, target, file);
            return Ok(new { path });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
