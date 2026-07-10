using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Clubuser
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ClubId { get; set; }

    public string ClubuserRole { get; set; } = null!;

    public virtual Club Club { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
