using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;

namespace LokalizacjaWSklepie.Pages;

public partial class ProductToBuyFromContainerPage : ContentPage
{

    public ProductToBuyFromContainerPage(List<Product> productsToBuy)
	{
		InitializeComponent();
        this.BindingContext = this;
        productsListView.ItemsSource = productsToBuy;

    }
    
    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}