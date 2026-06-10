using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Commentlike
{
    public int CommentId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Comment Comment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
