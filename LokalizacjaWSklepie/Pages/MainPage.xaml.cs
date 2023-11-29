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
        var ShopListPage = new ShopListPage();
        await Navigation.PushAsync(ShopListPage);
    }


    private async void Products_Clicked(object sender, EventArgs e)
    {
        var ProductsPage = new ProductsPage();
        await Navigation.PushAsync(ProductsPage);
    }
}

