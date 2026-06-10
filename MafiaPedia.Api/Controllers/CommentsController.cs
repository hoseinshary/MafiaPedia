using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MafiaPedia.Api.DTOs;
using MafiaPedia.Api.Services;

namespace MafiaPedia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("{entityType}/{entityId:int}")]
    [ProducesResponseType(typeof(List<CommentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<CommentResponseDto>>> GetComments(string entityType, int entityId)
    {
        if (!IsValidEntityType(entityType))
            return BadRequest("entityType must be 'player' or 'play'.");

        int? currentUserId = null;
        if (User.Identity?.IsAuthenticated == true)
            currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var comments = await _commentService.GetCommentsAsync(entityType, entityId, currentUserId);
        return Ok(comments);
    }

    [HttpPost("{entityType}/{entityId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommentResponseDto>> AddComment(string entityType, int entityId, [FromBody] CreateCommentRequestDto dto)
    {
        if (!IsValidEntityType(entityType))
            return BadRequest("entityType must be 'player' or 'play'.");

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Content is required.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var comment = await _commentService.AddCommentAsync(entityType, entityId, userId, dto);
            return CreatedAtAction(nameof(GetComments), new { entityType, entityId }, comment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{commentId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

        try
        {
            await _commentService.DeleteCommentAsync(commentId, userId, userRole);
            return Ok(new { message = "کامنت با موفقیت حذف شد" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{commentId:int}/like")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleLike(int commentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var isLiked = await _commentService.ToggleLikeAsync(commentId, userId);
            return Ok(new { isLiked, message = isLiked ? "لایک ثبت شد" : "لایک حذف شد" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private static bool IsValidEntityType(string entityType)
    {
        return entityType == "player" || entityType == "play";
    }
}
