using Microsoft.EntityFrameworkCore;
using MafiaPedia.Api.Data;
using MafiaPedia.Api.DTOs.Phase1;
using MafiaPedia.Api.IServices.Phase1;
using MafiaPedia.Api.Entities;

namespace MafiaPedia.Api.Services.Phase1;

public class CommentService : ICommentService
{
    private readonly MafiaDbContext _context;

    public CommentService(MafiaDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentResponseDto>> GetCommentsAsync(string entityType, int entityId, int? currentUserId = null)
    {
        var comments = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Commentlikes)
            .Include(c => c.InverseParentComment)
                .ThenInclude(r => r.User)
            .Include(c => c.InverseParentComment)
                .ThenInclude(r => r.Commentlikes)
            .Where(c => c.EntityType == entityType && c.EntityId == entityId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => MapToDto(c, currentUserId, c.InverseParentComment
            .OrderBy(r => r.CreatedAt)
            .Select(r => MapToDto(r, currentUserId, null))
            .ToList())).ToList();
    }

    public async Task<CommentResponseDto> AddCommentAsync(string entityType, int entityId, int userId, CreateCommentRequestDto dto)
    {
        if (dto.ParentCommentId.HasValue)
        {
            var parent = await _context.Comments.FindAsync(dto.ParentCommentId.Value);
            if (parent == null)
                throw new InvalidOperationException("Parent comment not found.");

            if (parent.EntityType != entityType || parent.EntityId != entityId)
                throw new InvalidOperationException("Parent comment does not belong to the same entity.");
        }

        var comment = new Comment
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            ParentCommentId = dto.ParentCommentId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        return MapToDto(comment, userId, null);
    }

    public async Task DeleteCommentAsync(int commentId, int userId, string userRole)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment is null)
            throw new KeyNotFoundException("کامنت یافت نشد");

        if (userRole != "admin" && comment.UserId != userId)
            throw new UnauthorizedAccessException("شما مجاز به حذف این کامنت نیستید");

        var replies = _context.Comments.Where(c => c.ParentCommentId == commentId);
        _context.Comments.RemoveRange(replies);

        var likes = _context.Commentlikes.Where(l => l.CommentId == commentId);
        _context.Commentlikes.RemoveRange(likes);

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ToggleLikeAsync(int commentId, int userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment is null)
            throw new KeyNotFoundException("کامنت یافت نشد");

        var existing = await _context.Commentlikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existing is not null)
        {
            _context.Commentlikes.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }
        else
        {
            _context.Commentlikes.Add(new Commentlike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }

    private static CommentResponseDto MapToDto(Comment c, int? currentUserId, List<CommentResponseDto>? replies)
    {
        return new CommentResponseDto
        {
            Id = c.Id,
            Content = c.Content,
            UserDisplayName = c.User.DisplayName ?? c.User.Username ?? "",
            UserId = c.UserId,
            ParentCommentId = c.ParentCommentId,
            CreatedAt = c.CreatedAt,
            LikeCount = c.Commentlikes.Count,
            IsLikedByCurrentUser = currentUserId.HasValue && c.Commentlikes.Any(l => l.UserId == currentUserId.Value),
            Replies = replies ?? new List<CommentResponseDto>()
        };
    }
}
