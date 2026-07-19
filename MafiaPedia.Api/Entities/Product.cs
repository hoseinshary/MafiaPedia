using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class Product
{
    public int Id { get; set; }

    public int ClubId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual Club Club { get; set; } = null!;

    public virtual ICollection<ClubOrderItem> ClubOrderItems { get; set; } = new List<ClubOrderItem>();

    public virtual User? DeletedByUser { get; set; }
}
