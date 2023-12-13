using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LokalizacjaWSklepie.Pages
{
    public partial class ProductsInContainerPage : ContentPage, INotifyPropertyChanged
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
        private List<Product> productsInContainer;
        private List<Product> allProducts;
        private int containerId;
        private int shopId;
        private string shopName;

        public ProductsInContainerPage(int containerId, int shopId, string shopName)
        {
            InitializeComponent();
            this.containerId = containerId;
            this.shopId = shopId;
            this.shopName = shopName;
            this.BindingContext = this;
            ProductsInContainer = new List<Product>();  
            AllProducts = new List<Product>();         
            LoadProductsAsync();
        }

        public List<Product> ProductsInContainer
        {
            get => productsInContainer;
            set
            {
                if (productsInContainer != value)
                {
                    productsInContainer = value;
                    OnPropertyChanged(nameof(ProductsInContainer));
                }
            }
        }

        public List<Product> AllProducts
        {
            get => allProducts;
            set
            {
                if (allProducts != value)
                {
                    allProducts = value;
                    OnPropertyChanged(nameof(AllProducts));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProductsInContainerCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Any())
            {
                var selectedProduct = e.CurrentSelection.First() as Product;

                ProductsInContainer.Remove(selectedProduct);
                AllProducts.Add(selectedProduct);
                OnPropertyChanged(nameof(ProductsInContainer));
                OnPropertyChanged(nameof(AllProducts));

                (sender as CollectionView).SelectedItem = null;

                
                ProductsInContainerCollectionView.ItemsSource = null;
                ProductsInContainerCollectionView.ItemsSource = ProductsInContainer;

                AllProductsCollectionView.ItemsSource = null;
                AllProductsCollectionView.ItemsSource = AllProducts;
            }
        }

        private void AllProductsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Any())
            {
                var selectedProduct = e.CurrentSelection.First() as Product;

                AllProducts.Remove(selectedProduct);
                ProductsInContainer.Add(selectedProduct);
                OnPropertyChanged(nameof(ProductsInContainer));
                OnPropertyChanged(nameof(AllProducts));

                (sender as CollectionView).SelectedItem = null;

                
                ProductsInContainerCollectionView.ItemsSource = null;
                ProductsInContainerCollectionView.ItemsSource = ProductsInContainer;

                AllProductsCollectionView.ItemsSource = null;
                AllProductsCollectionView.ItemsSource = AllProducts;
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                ProductsInContainer = await GetProductsInContainer(containerId);
                AllProducts = await GetAllProducts();

                
                AllProducts = AllProducts.Where(product => !ProductsInContainer.Any(pc => pc.ProductId == product.ProductId)).ToList();

                ProductsInContainerCollectionView.ItemsSource = ProductsInContainer;
                AllProductsCollectionView.ItemsSource = AllProducts;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<List<Product>> GetProductsInContainer(int containerId)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiBaseUrl}/api/Products/GetProductsInContainer/{containerId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Product>>(responseData);
                }
                else
                {
                    throw new Exception("Error while fetching products in container.");
                }
            }
        }

        private async Task<List<Product>> GetAllProducts()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiBaseUrl}/api/Products/GetAllProducts");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Product>>(responseData);
                }
                else
                {
                    throw new Exception("Error while fetching all products.");
                }
            }
        }
        private async void SaveChangesButton_Clicked(object sender, EventArgs e)
        {
            try
            {
               
                await RemoveAllProductsFromContainer();

                
                await AddProductsToContainer(ProductsInContainer);

               
                await LoadProductsAsync();
                await Navigation.PushAsync(new EditProductContainersPage(shopId, shopName));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task RemoveAllProductsFromContainer()
        {
            using (HttpClient client = new HttpClient())
            {
                await client.DeleteAsync($"{apiBaseUrl}/api/Products/RemoveAllProductsFromContainer/{containerId}");
            }
        }

        private async Task AddProductsToContainer(List<Product> products)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(products), Encoding.UTF8, "application/json");
                await client.PostAsync($"{apiBaseUrl}/api/Products/SaveProductsInContainer/{containerId}", content);
            }
        }
        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditProductContainersPage(shopId, shopName));
        }
        private void ProductsInContainerSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProductsInContainer(e.NewTextValue);
        }

        private void AllProductsSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAllProducts(e.NewTextValue);
        }

        private void FilterProductsInContainer(string searchText)
        {
            var filteredProducts = ProductsInContainer
                .Where(product => product.Name.ToLower().Contains(searchText.ToLower()) || product.Barcode.ToLower().Contains(searchText.ToLower()))
                .ToList();

            ProductsInContainerCollectionView.ItemsSource = filteredProducts;
        }

        private void FilterAllProducts(string searchText)
        {
            var filteredProducts = AllProducts
                .Where(product => product.Name.ToLower().Contains(searchText.ToLower()) || product.Barcode.ToLower().Contains(searchText.ToLower()))
                .ToList();

            AllProductsCollectionView.ItemsSource = filteredProducts;
        }

    }
}
