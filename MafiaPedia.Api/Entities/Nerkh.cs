using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Nerkh
{
    public int Id { get; set; }

    public int ClubId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsDefault { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public int? DefaultClubId { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<Clubplay> Clubplays { get; set; } = new List<Clubplay>();

    public virtual User? DeletedByUser { get; set; }
}
