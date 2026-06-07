namespace MafiaPedia.Api.DTOs;

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
