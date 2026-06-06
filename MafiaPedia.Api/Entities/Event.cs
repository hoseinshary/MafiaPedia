using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Event
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? ClubId { get; set; }

    public virtual Club? Club { get; set; }

    public virtual ICollection<Play> Plays { get; set; } = new List<Play>();
}
