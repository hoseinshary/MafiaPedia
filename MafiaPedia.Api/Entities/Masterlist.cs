using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Masterlist
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int MasteId { get; set; }

    public virtual Master Maste { get; set; } = null!;

    public virtual ICollection<MasterlistClubplayer> MasterlistClubplayers { get; set; } = new List<MasterlistClubplayer>();
}
