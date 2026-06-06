using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Club
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
