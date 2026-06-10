using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Comment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string EntityType { get; set; } = null!;

    public long EntityId { get; set; }

    public int? ParentCommentId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Commentlike> Commentlikes { get; set; } = new List<Commentlike>();

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual User User { get; set; } = null!;
}
