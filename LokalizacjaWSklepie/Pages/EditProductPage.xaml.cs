

using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;

namespace LokalizacjaWSklepie.Pages
{
    public partial class EditProductPage : ContentPage
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
        private Product product;

        public EditProductPage(Product product)
        {
            InitializeComponent();
            this.product = product;
            BindingContext = product;
        }

        private async void SaveChangesButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(product);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync($"{apiBaseUrl}/api/Products/UpdateProduct/{product.ProductId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Produkt zaktualizowany pomyœlnie", "OK");
                        var ProductsPage = new ProductsPage();
                        await Navigation.PushAsync(ProductsPage);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Nieudana próba edycji", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Deletion", "Czy jesteœ pewny ¿e chcesz usun¹æ produkt?", "Tak", "Nie");

            if (confirm)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.DeleteAsync($"{apiBaseUrl}/api/Products/DeleteProduct/{product.ProductId}");

                        if (response.IsSuccessStatusCode)
                        {
                            await DisplayAlert("Success", "Produkt usuniêty pomyœlnie", "OK");
                            var ProductsPage = new ProductsPage();
                            await Navigation.PushAsync(ProductsPage);
                        }
                        else
                        {
                            await DisplayAlert("Error", "Nieudana próba usuniêcia", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }
        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            var ProductsPage = new ProductsPage();
            await Navigation.PushAsync(ProductsPage);
        }
    }
}
