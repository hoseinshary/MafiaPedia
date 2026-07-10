using MafiaPedia.Api.DTOs.Phase1;

namespace MafiaPedia.Api.IServices.Phase1;

public interface IPlayerCommentService
{
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int playerId);
    Task<CommentDto> AddCommentAsync(int playerId, CreateCommentDto dto);
}
