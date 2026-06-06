using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class User
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Play> Plays { get; set; } = new List<Play>();
}
