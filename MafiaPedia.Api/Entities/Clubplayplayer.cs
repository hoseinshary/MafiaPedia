using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Clubplayplayer
{
    public int Id { get; set; }

    public int? Rank { get; set; }

    public int? Action { get; set; }

    public int RoleId { get; set; }

    public int PlayerId { get; set; }

    public int PlayId { get; set; }

    public bool IsGuest { get; set; }

    public int EntryCount { get; set; }

    public virtual Clubplay Play { get; set; } = null!;

    public virtual Clubplayer Player { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
