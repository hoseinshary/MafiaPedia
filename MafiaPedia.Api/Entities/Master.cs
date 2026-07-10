using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Master
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int ClubId { get; set; }

    public int? UserId { get; set; }

    public decimal? RatePerGame { get; set; }

    public string? Photo { get; set; }

    public string? Bio { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<Clubplay> Clubplays { get; set; } = new List<Clubplay>();

    public virtual ICollection<Masterlist> Masterlists { get; set; } = new List<Masterlist>();

    public virtual ICollection<Play> Plays { get; set; } = new List<Play>();

    public virtual User? User { get; set; }
}
