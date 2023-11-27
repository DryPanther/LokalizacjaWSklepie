using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LokalizacjaWSklepie.Pages
{
    public partial class ShopListPage : ContentPage
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;

        public ObservableCollection<Shop> Shops { get; set; }

        public ShopListPage()
        {
            InitializeComponent();
            Shops = new ObservableCollection<Shop>();
            LoadShops();
        }

        private async void LoadShops()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/api/Shops/GetAllShops");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var shops = JsonConvert.DeserializeObject<List<Shop>>(responseData);

                        foreach (var shop in shops)
                        {
                            Shops.Add(shop);
                        }

                        ShopsListView.ItemsSource = Shops;
                    }
                    else
                    {
                        await DisplayAlert("B≥πd", "Nie znaleziono sklepu", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("B≥πd", ex.Message, "OK");
            }
        }

        private async void ShopsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is Shop selectedShop)
                {
                    Console.WriteLine($"Item tapped: {selectedShop.Name}, ShopId: {selectedShop.ShopId}");

                    // Przejdü do strony edycji sklepu, przekazujπc ShopId
                    await Navigation.PushAsync(new MapEditorPage(selectedShop.ShopId, selectedShop.Name));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ShopsListView_ItemTapped: {ex.Message}");
            }
        }
    }
}
