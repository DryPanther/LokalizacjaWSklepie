using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;

namespace LokalizacjaWSklepie.Pages
{
    public partial class ProductsPage : ContentPage
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
        private ObservableCollection<Product> products;

        public ProductsPage()
        {
            InitializeComponent();
            products = new ObservableCollection<Product>();
            productsListView.ItemsSource = products;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync($"{apiBaseUrl}/api/Products/GetAllProducts");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var productList = JsonConvert.DeserializeObject<List<Product>>(responseData);

                        products.Clear();
                        foreach (var product in productList)
                        {
                            products.Add(product);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to retrieve products", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetAsync($"{apiBaseUrl}/api/Products/SearchProducts/{searchText}");

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            var productList = JsonConvert.DeserializeObject<List<Product>>(responseData);

                            products.Clear();
                            foreach (var product in productList)
                            {
                                products.Add(product);
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "Failed to retrieve products", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
            else
            {
                await LoadProducts();
            }
        }

        private async void OnProductSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null && e.SelectedItem is Product selectedProduct)
            {
                var editProductPage = new EditProductPage(selectedProduct);
                await Navigation.PushAsync(editProductPage);

                ((ListView)sender).SelectedItem = null;
            }
        }

        private async void AddProduct_Clicked(object sender, EventArgs e)
        {
            var addProductPage = new AddProductPage();
            await Navigation.PushAsync(addProductPage);
        }
        private async void Back_Clicked(object sender, EventArgs e)
        {
            var MainPage = new MainPage();
            await Navigation.PushAsync(MainPage);
        }
    }
}
