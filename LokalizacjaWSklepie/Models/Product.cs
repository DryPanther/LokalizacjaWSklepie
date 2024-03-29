﻿namespace LokalizacjaWSklepie.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; }

    public double BasePrice { get; set; }

    public string Barcode { get; set; }

    public string QuantityType { get; set; }

    public virtual ICollection<ProductQuantity> ProductQuantities { get; set; } = new List<ProductQuantity>();

    public virtual ICollection<ShoppingListProduct> ShoppingListProducts { get; set; } = new List<ShoppingListProduct>();

    public virtual ICollection<Container> Containers { get; set; } = new List<Container>();
}
