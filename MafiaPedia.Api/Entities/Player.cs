using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Player
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Mobile { get; set; }

    public DateOnly? Birthday { get; set; }

    public string? Code { get; set; }

    public string? Picture { get; set; }

    public string? Desc { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Playplayer> Playplayers { get; set; } = new List<Playplayer>();

    public virtual User? User { get; set; }
}
