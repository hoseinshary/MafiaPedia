using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Playplayer
{
    public int Id { get; set; }

    public int? Rank { get; set; }

    public int? Action { get; set; }

    public int RoleId { get; set; }

    public int PlayerId { get; set; }

    public int PlayId { get; set; }

    public virtual Play Play { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
