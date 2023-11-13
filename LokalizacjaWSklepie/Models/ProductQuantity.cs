using System;
using System.Collections.Generic;

namespace LokalizacjaWSklepie.Models;

public partial class ProductQuantity
{
    public int ShopId { get; set; }

    public int ProductId { get; set; }

    public double Quantity { get; set; }

    public virtual Product Product { get; set; }

    public virtual Shop Shop { get; set; }
}
