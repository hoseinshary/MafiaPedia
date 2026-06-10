using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? PasswordSalt { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public string Mobile { get; set; } = null!;

    public bool? MobileVerified { get; set; }

    public string? OtpCode { get; set; }

    public DateTime? OtpExpiresAt { get; set; }

    public string? DisplayName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<Commentlike> Commentlikes { get; set; } = new List<Commentlike>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Player? Player { get; set; }

    public virtual ICollection<Play> Plays { get; set; } = new List<Play>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
