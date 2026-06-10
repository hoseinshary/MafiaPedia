using MafiaPedia.Api.DTOs;

namespace MafiaPedia.Api.Services.Iservices;

public interface IPlayerCommentService
{
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int playerId);
    Task<CommentDto> AddCommentAsync(int playerId, CreateCommentDto dto);
}
