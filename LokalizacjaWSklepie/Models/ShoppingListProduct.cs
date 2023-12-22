namespace LokalizacjaWSklepie.Models;

public partial class ShoppingListProduct
{
    public int ShoppingListId { get; set; }

    public int ProductId { get; set; }

    public double? Quantity { get; set; }

    public virtual Product Product { get; set; }

    public virtual ShoppingList ShoppingList { get; set; }
}
