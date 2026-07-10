using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Room
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int ClubId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<Clubplay> Clubplays { get; set; } = new List<Clubplay>();

    public virtual ICollection<Play> Plays { get; set; } = new List<Play>();
}
