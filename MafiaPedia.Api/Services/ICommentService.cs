using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetCommentsAsync(string entityType, int entityId, int? currentUserId = null);
    Task<CommentResponseDto> AddCommentAsync(string entityType, int entityId, int userId, CreateCommentRequestDto dto);
    Task DeleteCommentAsync(int commentId, int userId, string userRole);
    Task<bool> ToggleLikeAsync(int commentId, int userId);
}
