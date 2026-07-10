using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Clubplayer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string Mobile { get; set; } = null!;

    public DateOnly? Birthday { get; set; }

    public string? Code { get; set; }

    public string? Picture { get; set; }

    public string? Desc { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Clubplayplayer> Clubplayplayers { get; set; } = new List<Clubplayplayer>();

    public virtual ICollection<MasterlistClubplayer> MasterlistClubplayers { get; set; } = new List<MasterlistClubplayer>();

    public virtual User? User { get; set; }
}
