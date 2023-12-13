using LokalizacjaWSklepie.Pages;

namespace LokalizacjaWSklepie;

public partial class MainPage : ContentPage
{


    public MainPage()
    {
        InitializeComponent();
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
}

