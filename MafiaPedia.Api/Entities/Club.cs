using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Club
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? City { get; set; }

    public string? Description { get; set; }

    public string? Logo { get; set; }

    public virtual ICollection<Clubuser> Clubusers { get; set; } = new List<Clubuser>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Master> Masters { get; set; } = new List<Master>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
