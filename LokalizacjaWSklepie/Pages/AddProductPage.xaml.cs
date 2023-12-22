using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;
/* Niescalona zmiana z projektu „LokalizacjaWSklepie (net7.0-ios)”
Przed:
using System.Threading.Tasks;
using LokalizacjaWSklepie.Properties;
Po:
using System.Threading.Tasks;
*/

/* Niescalona zmiana z projektu „LokalizacjaWSklepie (net7.0-windows10.0.19041.0)”
Przed:
using System.Threading.Tasks;
using LokalizacjaWSklepie.Properties;
Po:
using System.Threading.Tasks;
*/

/* Niescalona zmiana z projektu „LokalizacjaWSklepie (net7.0-maccatalyst)”
Przed:
using System.Threading.Tasks;
using LokalizacjaWSklepie.Properties;
Po:
using System.Threading.Tasks;
*/


namespace LokalizacjaWSklepie.Pages
{
    public partial class AddProductPage : ContentPage
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;

        public AddProductPage()
        {
            InitializeComponent();
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var newProduct = new Product
                    {
                        Name = productNameEntry.Text,
                        BasePrice = Convert.ToDouble(basePriceEntry.Text),
                        Barcode = barcodeEntry.Text,
                        QuantityType = quantityTypePicker.SelectedItem.ToString()
                    };

                    string json = JsonConvert.SerializeObject(newProduct);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{apiBaseUrl}/api/Products/AddProduct", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Product zosta³ dodany.", "OK");
                        var ProductsPage = new ProductsPage();
                        await Navigation.PushAsync(ProductsPage);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Nie uda³o siê dodaæ produktu.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private async void Back_Clicked(object sender, EventArgs e)
        {
            var ProductsPage = new ProductsPage();
            await Navigation.PushAsync(ProductsPage);
        }
    }
}
