using System;
using System.Collections.Generic;

namespace LokalizacjaWSklepie.Models;

public partial class Container
{
    public int ContainerId { get; set; }

    public int? ShopId { get; set; }

    public string ContainerType { get; set; }

    public double Width { get; set; }

    public double Length { get; set; }

    public int? CoordinateX { get; set; }

    public int? CoordinateY { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
