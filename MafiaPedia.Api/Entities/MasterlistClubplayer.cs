using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class MasterlistClubplayer
{
    public int Id { get; set; }

    public int ClubplayerId { get; set; }

    public int MasterlistId { get; set; }

    public virtual Clubplayer Clubplayer { get; set; } = null!;

    public virtual Masterlist Masterlist { get; set; } = null!;
}
