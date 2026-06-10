using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Subscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Plan { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual User User { get; set; } = null!;
}
