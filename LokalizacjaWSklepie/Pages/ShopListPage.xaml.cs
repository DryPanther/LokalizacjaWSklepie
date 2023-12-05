using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;

namespace LokalizacjaWSklepie.Pages
{
    public partial class ShopListPage : ContentPage
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
        private ObservableCollection<Shop> shops;
        private string chose;

        public ShopListPage(string choice)
        {
            InitializeComponent();
            shops = new ObservableCollection<Shop>();
            ShopsListView.ItemsSource = shops;
            LoadShops();
            chose = choice;
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
                        var shopList = JsonConvert.DeserializeObject<List<Shop>>(responseData);

                        shops.Clear();
                        foreach (var shop in shopList)
                        {
                            shops.Add(shop);
                        }

                        ShopsListView.ItemsSource = shops;
                    }
                    else
                    {
                        await DisplayAlert("B��d", "Nie znaleziono sklepu", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("B��d", ex.Message, "OK");
            }
        }

        private async void ShopsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is Shop selectedShop)
                {
                    if (chose == "EditShops")
                    {
                        Console.WriteLine($"Item tapped: {selectedShop.Name}, ShopId: {selectedShop.ShopId}");

                        await Navigation.PushAsync(new MapEditorPage(selectedShop.ShopId, selectedShop.Name));
                    }
                    if (chose == "EditProductContaiiners")
                    {
                        Console.WriteLine($"Item tapped: {selectedShop.Name}, ShopId: {selectedShop.ShopId}");

                        await Navigation.PushAsync(new EditProductContainersPage(selectedShop.ShopId, selectedShop.Name));
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ShopsListView_ItemTapped: {ex.Message}");
            }
        }

        private async void Create_Clicked(object sender, EventArgs e)
        {
            var MapCreator = new MapCreator();
            await Navigation.PushAsync(MapCreator);
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            var MainPage = new MainPage();
            await Navigation.PushAsync(MainPage);
        }

        private async void OnShopSearchButtonPressed(object sender, EventArgs e)
        {
            string searchText = shopSearchBar.Text;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                await SearchShops(searchText);
            }
            else
            {
                LoadShops();
            }
        }

        private async void OnShopSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                await SearchShops(searchText);
            }
            else
            {
                LoadShops();
            }
        }

        private async Task SearchShops(string searchText)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/api/Shops/SearchShops/{searchText}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var shopList = JsonConvert.DeserializeObject<List<Shop>>(responseData);

                        shops.Clear();
                        foreach (var shop in shopList)
                        {
                            shops.Add(shop);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to retrieve shops", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
