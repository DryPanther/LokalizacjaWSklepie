using System;
using System.Collections.Generic;

namespace LokalizacjaWSklepie.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; }

    public double BasePrice { get; set; }

    public string Barcode { get; set; }

    public string QuantityType { get; set; }

    public virtual ICollection<ProductContainer> ProductContainers { get; set; } = new List<ProductContainer>();

    public virtual ICollection<ProductQuantity> ProductQuantities { get; set; } = new List<ProductQuantity>();
}
