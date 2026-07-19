using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class ClubSettlement
{
    public int Id { get; set; }

    public int ClubId { get; set; }

    public int ClubPlayerId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public int? OrderId { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual Clubplayer ClubPlayer { get; set; } = null!;

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual User? DeletedByUser { get; set; }

    public virtual ClubOrder? Order { get; set; }
}
