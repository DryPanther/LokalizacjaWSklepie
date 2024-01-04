namespace LokalizacjaWSklepie.Pages;

public partial class ClientMenuPage : ContentPage
{
	public ClientMenuPage()
	{
		InitializeComponent();
        Title = Memory.Instance.user.Username;
    }
    private async void ProductSearch_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("Search");
        await Navigation.PushAsync(ShopListPage);
    }
    private async void ShoppingLists_Clicked(object sender, EventArgs e)
    {
        var ShoppingListListPage = new ShoppingListListPage();
        await Navigation.PushAsync(ShoppingListListPage);
    }
    private async void Logout_Clicked(object sender, EventArgs e)
    {
        Memory.Instance.user = null;
        await Shell.Current.Navigation.PopToRootAsync();
        await Task.Delay(100);
        Shell.Current.Navigation.RemovePage(Shell.Current.CurrentPage);
        var stack = Shell.Current.Navigation.NavigationStack.ToArray();
        for (int i = stack.Length - 1; i > 0; i--)
        {
            Shell.Current.Navigation.RemovePage(stack[i]);
        }
    }
}