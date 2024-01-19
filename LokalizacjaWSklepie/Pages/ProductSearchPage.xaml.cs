using LokalizacjaWSklepie.Extensions;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using Container = LokalizacjaWSklepie.Models.Container;

namespace LokalizacjaWSklepie.Pages;

public partial class ProductSearchPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private int skala = 100;
    private double skala1 = 100;
    private int shopId;
    private string name;
    private List<Container> currentContainers;
    private List<Product> allProducts;
    private List<Product> searchResults;
    private bool isSearchBarFocused = false;

    public ProductSearchPage(int shopId, string name)
    {
        Title = name;
        InitializeComponent();
        LoadProducts();
        productsListView.IsVisible = false;
        this.shopId = shopId;
        this.name = name;
        LoadMap();
    }
    private async void LoadProducts()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiBaseUrl}/api/Products/GetAllProducts");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    allProducts = JsonConvert.DeserializeObject<List<Product>>(responseData);

                    searchResults = allProducts;
                    productsListView.ItemsSource = searchResults;
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

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue;


        if (!string.IsNullOrWhiteSpace(searchText))
        {
            SearchProducts(searchText);
            productsListView.IsVisible = true;

        }
        else
        {
            productsListView.IsVisible = false;
        }
    }

    private void SearchProducts(string searchText)
    {
        if (allProducts == null)
            return;
        if (searchText == "")
        {
            searchResults = allProducts;
            return;
        }
        else
        {
            searchResults = allProducts
                .FindAll(p => p.Name.ToLower().Contains(searchText.ToLower()) || p.Barcode.ToLower().Contains(searchText.ToLower()));
        }


        productsListView.ItemsSource = searchResults;
    }

    private async void OnProductSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem != null && e.SelectedItem is Product selectedProduct)
        {
            try
            {
                foreach (var child in Layout.Children)
                {
                    if (child is BoxViewExtensions containerBox)
                    {
                        var label = new Label
                        {

                        };
                        containerBox.BorderColor = Colors.Black;
                        containerBox.Content = label;
                    }
                }
                var containerIds = await GetDistinctContainerIds(selectedProduct.ProductId, shopId);

                foreach (var containerId in containerIds)
                {

                    foreach (var child in Layout.Children)
                    {
                        if (child is BoxViewExtensions containerBox)
                        {
                            int containerBoxId = BoxViewExtensions.GetId(containerBox);
                            if (containerBoxId == containerId)
                            {

                                var label = new Label
                                {
                                    Text = selectedProduct.Name,
                                    TextColor = Colors.White,
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    VerticalTextAlignment = TextAlignment.Center,
                                    FontAttributes = FontAttributes.Bold,
                                };
                                containerBox.BorderColor = Colors.Red;
                                containerBox.Content = label;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }

            ((ListView)sender).SelectedItem = null;
        }
    }
    private async Task<List<int>> GetDistinctContainerIds(int productId, int shopId)
    {
        using (HttpClient client = new HttpClient())
        {

            string endpoint = $"{apiBaseUrl}/api/Products/GetDistinctContainerIds/{productId}/{shopId}";

            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<int>>(responseData);
            }
            else
            {
                throw new Exception("Error while fetching distinct container IDs.");
            }
        }
    }

    private void OnSearchBarFocused(object sender, EventArgs e)
    {
        isSearchBarFocused = true;
        productsListView.IsVisible = true;
    }

    private void OnSearchBarUnfocused(object sender, EventArgs e)
    {
        isSearchBarFocused = false;
        productsListView.IsVisible = false;
    }
    private async Task LoadMap()
    {
        try
        {
            var shopMapData = await GetShopMapDataFromDatabase();

            if (shopMapData != null)
            {
                var shopBox = CreateShopBox(shopMapData.Width, shopMapData.Length);
                Layout.Children.Add(shopBox);

                BoxViewExtensions.SetId(shopBox, shopMapData.ShopId);

                var containers = await GetContainersByShopIdFromDatabase(shopMapData.ShopId);

                foreach (var containerData in containers)
                {
                    var containerBox = CreateContainerBox(containerData.Width, containerData.Length, (int)containerData.CoordinateX, (int)containerData.CoordinateY, containerData.ContainerType);

                    BoxViewExtensions.SetId(containerBox, containerData.ContainerId);

                    Layout.Children.Add(containerBox);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas wczytywania mapy: {ex.Message}");
        }
    }

    private BoxViewExtensions CreateShopBox(double width, double length)
    {
        var shopBox = new BoxViewExtensions
        {
            BackgroundColor = Colors.LightGray,
            WidthRequest = width * skala,
            HeightRequest = length * skala
        };
        shopBox.ClassId = "Sklep";
        Layout.SetRow(shopBox, 1);
        return shopBox;
    }

    private Frame CreateContainerBox(double width, double length, int coordinateX, int coordinateY, string containerType)
    {

        var containerBox = new BoxViewExtensions
        {
            WidthRequest = width * skala,
            HeightRequest = length * skala,
            CornerRadius = 10,
            BorderColor = Colors.Black,
        };


        containerBox.TranslationX = coordinateX;
        containerBox.TranslationY = coordinateY;
        containerBox.ClassId = containerType;

        switch (containerBox.ClassId)
        {
            case "Pó³ka":
                containerBox.BackgroundColor = Colors.BurlyWood; break;

            case "Lodówka":
                containerBox.BackgroundColor = Colors.SkyBlue; break;
            case "Zamra¿arka":
                containerBox.BackgroundColor = Colors.DeepSkyBlue; break;
            case "Stojak":
                containerBox.BackgroundColor = Colors.SaddleBrown; break;
            case "Kasa":
                containerBox.BackgroundColor = Colors.Gold; break;
            default:
                break;
        }

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += ShelfTapped;
        containerBox.GestureRecognizers.Add(tapGesture);
        Layout.SetRow(containerBox, 1);
        return containerBox;
    }
    private async Task<Shop> GetShopMapDataFromDatabase()
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync($"{apiBaseUrl}/api/Shops/GetShopById/{shopId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Shop>(responseData);
            }
            else
            {
                throw new Exception("B³¹d podczas pobierania danych mapy ze sklepu.");
            }
        }
    }
    private async Task<List<Container>> GetContainersByShopIdFromDatabase(int shopId)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync($"{apiBaseUrl}/api/Containers/GetContainersByShopId/{shopId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Container>>(responseData);
            }
            else
            {
                throw new Exception("B³¹d podczas pobierania pojemnika z bazy danych.");
            }
        }
    }
    private async void ShelfTapped(object sender, EventArgs e)
    {
        if (sender is BoxViewExtensions selectedShelf)
        {
            int containerId = BoxViewExtensions.GetId(selectedShelf);
            await Navigation.PushAsync(new ProductsInContainerListPage(containerId));
        }
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("Search");
        await Navigation.PushAsync(ShopListPage);
    }
    private void ScaleAdd_Clicked(object sender, EventArgs e)
    {
        if (skala1 < 150)
        {
            foreach (var item in Layout.OfType<BoxViewExtensions>())
            {
                item.WidthRequest = ((item.Width * 100) / skala1) * ((skala1 + 10) * 0.01);
                item.HeightRequest = ((item.Height * 100) / skala1) * ((skala1 + 10) * 0.01);
                if (item.ClassId != "Sklep")
                {
                    item.TranslationX = ((item.TranslationX * 100) / skala1) * ((skala1 + 10) * 0.01);
                    item.TranslationY = ((item.TranslationY * 100) / skala1) * ((skala1 + 10) * 0.01);
                }
            }
            skala1 += 10;
            Scale.Text = Convert.ToString(skala1) + "%";
        }

    }
    private void ScaleSub_Clicked(object sender, EventArgs e)
    {
        if (skala1 > 40)
        {
            foreach (var item in Layout.OfType<BoxViewExtensions>())
            {
                item.WidthRequest = ((item.Width * 100) / skala1) * ((skala1 - 10) * 0.01);
                item.HeightRequest = ((item.Height * 100) / skala1) * ((skala1 - 10) * 0.01);
                if (item.ClassId != "Sklep")
                {
                    item.TranslationX = ((item.TranslationX * 100) / skala1) * ((skala1 - 10) * 0.01);
                    item.TranslationY = ((item.TranslationY * 100) / skala1) * ((skala1 - 10) * 0.01);
                }
            }
            skala1 -= 10;
            Scale.Text = Convert.ToString(skala1) + "%";
        }

    }

}
