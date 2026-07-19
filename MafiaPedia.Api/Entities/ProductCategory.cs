using System;
using System.Collections.Generic;

namespace MafiaPedia.Api.Entities;

public partial class ProductCategory
{
    public int Id { get; set; }

    public int ClubId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public virtual Club Club { get; set; } = null!;

    public virtual User? DeletedByUser { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
