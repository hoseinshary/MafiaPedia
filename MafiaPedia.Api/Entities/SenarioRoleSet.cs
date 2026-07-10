using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class SenarioRoleSet
{
    public int Id { get; set; }

    public int SenarioId { get; set; }

    public int PlayerCount { get; set; }

    public string RoleIds { get; set; } = null!;

    public virtual Senario Senario { get; set; } = null!;
}
