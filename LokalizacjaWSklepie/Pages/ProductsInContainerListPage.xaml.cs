using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;

namespace LokalizacjaWSklepie.Pages;

public partial class ProductsInContainerListPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private List<Product> productsInContainer;
    private List<Product> allProducts;
    private int containerId;
    private int shopId;
    private string shopName;
    public ProductsInContainerListPage(int containerId, int shopId, string shopName)
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
    private async Task LoadProductsAsync()
    {
        try
        {
            ProductsInContainer = await GetProductsInContainer(containerId);


            AllProducts = AllProducts.Where(product => !ProductsInContainer.Any(pc => pc.ProductId == product.ProductId)).ToList();

            ProductsInContainerCollectionView.ItemsSource = ProductsInContainer;
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
    private void ProductsInContainerSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterProductsInContainer(e.NewTextValue);
    }
    private void FilterProductsInContainer(string searchText)
    {
        var filteredProducts = ProductsInContainer
            .Where(product => product.Name.ToLower().Contains(searchText.ToLower()) || product.Barcode.ToLower().Contains(searchText.ToLower()))
            .ToList();

        ProductsInContainerCollectionView.ItemsSource = filteredProducts;
    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProductSearchPage(shopId, shopName));
    }
}