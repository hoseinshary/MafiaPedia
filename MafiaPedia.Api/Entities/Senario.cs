using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Senario
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Clubplay> Clubplays { get; set; } = new List<Clubplay>();

    public virtual ICollection<Play> Plays { get; set; } = new List<Play>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<SenarioRoleSet> SenarioRoleSets { get; set; } = new List<SenarioRoleSet>();
}
