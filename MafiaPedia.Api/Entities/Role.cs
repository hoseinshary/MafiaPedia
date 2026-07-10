using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Role
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int SenarioId { get; set; }

    public int SideId { get; set; }

    public string? Photo { get; set; }

    public virtual ICollection<Clubplayplayer> Clubplayplayers { get; set; } = new List<Clubplayplayer>();

    public virtual ICollection<Playplayer> Playplayers { get; set; } = new List<Playplayer>();

    public virtual Senario Senario { get; set; } = null!;

    public virtual Side Side { get; set; } = null!;
}
