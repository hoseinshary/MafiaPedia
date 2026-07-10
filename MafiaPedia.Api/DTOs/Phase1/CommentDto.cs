namespace MafiaPedia.Api.DTOs.Phase1;

public class CommentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
}

public class CreateCommentDto
{
    public int UserId { get; set; } = 1;
    public string Content { get; set; } = string.Empty;
}

public class CreateCommentRequestDto
{
    public string Content { get; set; } = null!;
    public int? ParentCommentId { get; set; }
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string UserDisplayName { get; set; } = null!;
    public int UserId { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public List<CommentResponseDto> Replies { get; set; } = new();
}
