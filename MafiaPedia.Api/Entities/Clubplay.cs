using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Clubplay
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public DateTime? DateTime { get; set; }

    public int? PlayersCount { get; set; }

    public int? GuestCount { get; set; }

    public string? Desc { get; set; }

    public int RoomId { get; set; }

    public int SenarioId { get; set; }

    public int MasterId { get; set; }

    public int? WinnersideId { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public string? Link { get; set; }

    public string PlayType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly? BusinessDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public int? NerkhId { get; set; }

    public virtual ICollection<Clubplayplayer> Clubplayplayers { get; set; } = new List<Clubplayplayer>();

    public virtual User? DeletedByUser { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Master Master { get; set; } = null!;

    public virtual Nerkh? Nerkh { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual Senario Senario { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Side? Winnerside { get; set; }
}
