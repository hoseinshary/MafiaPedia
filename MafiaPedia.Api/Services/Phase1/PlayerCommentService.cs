using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.Entities;
using MafiaPedia.Api.IServices.Phase1;

namespace MafiaPedia.Api.Services.Phase1;

public class PlayerCommentService : IPlayerCommentService
{
    private readonly MafiaDbContext _context;

    public PlayerCommentService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int playerId)
    {
        return await _context.Comments
            .Where(c => c.EntityType == "Player" && c.EntityId == playerId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.User.Username ?? "",
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                LikeCount = c.Commentlikes.Count,
                ReplyCount = c.InverseParentComment.Count
            })
            .ToListAsync();
    }

    public async Task<CommentDto> AddCommentAsync(int playerId, CreateCommentDto dto)
    {
        var comment = new Comment
        {
            UserId = dto.UserId,
            EntityType = "Player",
            EntityId = playerId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(comment.UserId);

        return new CommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = user?.Username ?? "",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            LikeCount = 0,
            ReplyCount = 0
        };
    }
}
