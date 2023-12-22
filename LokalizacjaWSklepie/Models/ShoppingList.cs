namespace LokalizacjaWSklepie.Models;

public partial class ShoppingList
{
    public int ShoppingListId { get; set; }

    public int UserId { get; set; }

    public string ListName { get; set; }

    public virtual ICollection<ShoppingListProduct> ShoppingListProducts { get; set; } = new List<ShoppingListProduct>();

    public virtual User User { get; set; }
}
