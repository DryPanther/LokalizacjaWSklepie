namespace LokalizacjaWSklepie.Models;

public partial class Shop
{
    public int ShopId { get; set; }

    public string Name { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string PostalCode { get; set; }

    public double Width { get; set; }

    public double Length { get; set; }

    public virtual ICollection<Container> Containers { get; set; } = new List<Container>();

    public virtual ICollection<ProductQuantity> ProductQuantities { get; set; } = new List<ProductQuantity>();
}
