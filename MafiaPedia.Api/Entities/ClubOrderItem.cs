using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class ClubOrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public virtual User? DeletedByUser { get; set; }

    public virtual ClubOrder Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
