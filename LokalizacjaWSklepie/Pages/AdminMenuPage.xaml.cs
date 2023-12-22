namespace LokalizacjaWSklepie.Pages;

public partial class AdminMenuPage : ContentPage
{
    public AdminMenuPage()
    {
        InitializeComponent();

        Title = Memory.Instance.user.Username;
    }
    private async void Shops_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("EditShops");
        await Navigation.PushAsync(ShopListPage);
    }


    private async void Products_Clicked(object sender, EventArgs e)
    {
        var ProductsPage = new ProductsPage();
        await Navigation.PushAsync(ProductsPage);
    }

    private async void ProductContainers_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("EditProductContaiiners");
        await Navigation.PushAsync(ShopListPage);
    }

    private async void ProductSearch_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("Search");
        await Navigation.PushAsync(ShopListPage);
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

    private async void Users_Clicked(object sender, EventArgs e)
    {
        var UserListPage = new UserListPage();
        await Navigation.PushAsync(UserListPage);
    }
}
