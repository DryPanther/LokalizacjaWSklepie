using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace LokalizacjaWSklepie.Pages;

public partial class ShoppingListEditPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private List<Product> productsOnShoppingList;
    private List<Product> allProducts;
    private int shoppingListId;
    public ShoppingListEditPage(int shoppingListId)
	{
		InitializeComponent();
        double screenHeight = DeviceDisplay.MainDisplayInfo.Height;
        double gridHeightPercentage = 0.12;
        Scroll1.HeightRequest = screenHeight * gridHeightPercentage;
        Scroll2.HeightRequest = screenHeight * gridHeightPercentage;
        this.shoppingListId = shoppingListId;
        ProductsOnShoppingList = new List<Product>();
        AllProducts = new List<Product>();
        LoadProductsAsync();
        
    }


    public List<Product> ProductsOnShoppingList
    {
        get => productsOnShoppingList;
        set
        {
            if (productsOnShoppingList != value)
            {
                productsOnShoppingList = value;
                OnPropertyChanged(nameof(productsOnShoppingList));
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

    private void ProductsOnShoppingListCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Any())
        {
            var selectedProduct = e.CurrentSelection.First() as Product;

            ProductsOnShoppingList.Remove(selectedProduct);
            AllProducts.Add(selectedProduct);
            OnPropertyChanged(nameof(ProductsOnShoppingList));
            OnPropertyChanged(nameof(AllProducts));

            (sender as CollectionView).SelectedItem = null;


            ProductsOnShoppingListCollectionView.ItemsSource = null;
            ProductsOnShoppingListCollectionView.ItemsSource = ProductsOnShoppingList;

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
            ProductsOnShoppingList.Add(selectedProduct);
            OnPropertyChanged(nameof(ProductsOnShoppingList));
            OnPropertyChanged(nameof(AllProducts));
            (sender as CollectionView).SelectedItem = null;


            ProductsOnShoppingListCollectionView.ItemsSource = null;
            ProductsOnShoppingListCollectionView.ItemsSource = ProductsOnShoppingList;

            AllProductsCollectionView.ItemsSource = null;
            AllProductsCollectionView.ItemsSource = AllProducts;
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            ProductsOnShoppingList = await GetProductsOnShoppingList(shoppingListId);
            AllProducts = await GetAllProducts();


            AllProducts = AllProducts.Where(product => !ProductsOnShoppingList.Any(pc => pc.ProductId == product.ProductId)).ToList();

            ProductsOnShoppingListCollectionView.ItemsSource = ProductsOnShoppingList;
            AllProductsCollectionView.ItemsSource = AllProducts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async Task<List<Product>> GetProductsOnShoppingList(int shoppingListId)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync($"{apiBaseUrl}/api/ShoppingLists/GetProductsOnShoppingList/{shoppingListId}");

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

            await RemoveAllProductsFromShoppingList();


            await AddProductsToShoppingList(ProductsOnShoppingList);


            await LoadProductsAsync();
            await Navigation.PushAsync(new ShoppingListListPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async Task RemoveAllProductsFromShoppingList()
    {
        using (HttpClient client = new HttpClient())
        {
            await client.DeleteAsync($"{apiBaseUrl}/api/ShoppingLists/RemoveAllProductsFromShoppingList/{shoppingListId}");
        }
    }

    private async Task AddProductsToShoppingList(List<Product> products)
    {
        using (HttpClient client = new HttpClient())
        {
            var content = new StringContent(JsonConvert.SerializeObject(products), Encoding.UTF8, "application/json");
            await client.PostAsync($"{apiBaseUrl}/api/ShoppingLists/SaveProductsOnShoppingList/{shoppingListId}", content);
        }
    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ShoppingListListPage());
    }
    private void ProductsOnShoppingListSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterProductsInContainer(e.NewTextValue);
    }

    private void AllProductsSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterAllProducts(e.NewTextValue);
    }

    private void FilterProductsInContainer(string searchText)
    {
        var filteredProducts = ProductsOnShoppingList
            .Where(product => product.Name.ToLower().Contains(searchText.ToLower()) || product.Barcode.ToLower().Contains(searchText.ToLower()))
            .ToList();

        ProductsOnShoppingListCollectionView.ItemsSource = filteredProducts;
    }

    private void FilterAllProducts(string searchText)
    {
        var filteredProducts = AllProducts
            .Where(product => product.Name.ToLower().Contains(searchText.ToLower()) || product.Barcode.ToLower().Contains(searchText.ToLower()))
            .ToList();

        AllProductsCollectionView.ItemsSource = filteredProducts;
    }
}