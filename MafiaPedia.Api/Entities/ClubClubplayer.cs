using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class ClubClubplayer
{
    public int ClubId { get; set; }

    public int ClubplayerId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual Clubplayer Clubplayer { get; set; } = null!;
}
