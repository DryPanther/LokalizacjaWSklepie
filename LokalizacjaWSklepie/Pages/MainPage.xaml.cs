using LokalizacjaWSklepie.Pages;

namespace LokalizacjaWSklepie;

public partial class MainPage : ContentPage
{


    public MainPage()
    {
        InitializeComponent();
    }


    private async void Create_Clicked(object sender, EventArgs e)
    {

        var mapCreator = new MapCreator();
        await Navigation.PushAsync(mapCreator);
    }

    private async void Edit_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage();
        await Navigation.PushAsync(ShopListPage);
    }
}

