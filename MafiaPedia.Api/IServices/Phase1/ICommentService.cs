using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetCommentsAsync(string entityType, int entityId, int? currentUserId = null);
    Task<CommentResponseDto> AddCommentAsync(string entityType, int entityId, int userId, CreateCommentRequestDto dto);
    Task DeleteCommentAsync(int commentId, int userId, string userRole);
    Task<bool> ToggleLikeAsync(int commentId, int userId);
}
