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

    public decimal? VatPercent { get; set; }

    public virtual ICollection<ClubOrder> ClubOrders { get; set; } = new List<ClubOrder>();

    public virtual ICollection<ClubSettlement> ClubSettlements { get; set; } = new List<ClubSettlement>();

    public virtual ICollection<Clubuser> Clubusers { get; set; } = new List<Clubuser>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Master> Masters { get; set; } = new List<Master>();

    public virtual ICollection<Nerkh> Nerkhs { get; set; } = new List<Nerkh>();

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
