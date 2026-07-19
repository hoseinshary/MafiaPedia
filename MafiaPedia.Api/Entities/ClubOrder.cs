using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class ClubOrder
{
    public int Id { get; set; }

    public int ClubId { get; set; }

    public int ClubPlayerId { get; set; }

    public DateOnly BusinessDate { get; set; }

    public int RegisteredByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SettledAt { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<ClubOrderItem> ClubOrderItems { get; set; } = new List<ClubOrderItem>();

    public virtual Clubplayer ClubPlayer { get; set; } = null!;

    public virtual ICollection<ClubSettlement> ClubSettlements { get; set; } = new List<ClubSettlement>();

    public virtual User? DeletedByUser { get; set; }

    public virtual User RegisteredByUser { get; set; } = null!;
}
