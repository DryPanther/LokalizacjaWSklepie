using LokalizacjaWSklepie.Extensions;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;

namespace LokalizacjaWSklepie.Pages;

public partial class EditProductContainersPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private int skala = 100;
    private int shopId;
    private string name;
    public EditProductContainersPage(int shopId, string name)
    {
        Title = name;
        InitializeComponent();
        this.shopId = shopId;
        this.name = name;
        LoadMap();
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

    private BoxView CreateShopBox(double width, double length)
    {
        var shopBox = new BoxView
        {
            Color = Colors.LightGray,
            WidthRequest = width * skala,
            HeightRequest = length * skala
        };
        shopBox.ClassId = "Sklep";
        Layout.SetRow(shopBox, 1);
        return shopBox;
    }

    private BoxView CreateContainerBox(double width, double length, int coordinateX, int coordinateY, string containerType)
    {
        var containerBox = new BoxViewExtensions
        {
            WidthRequest = width * skala,
            HeightRequest = length * skala,
            CornerRadius = new CornerRadius(10)
        };

        containerBox.TranslationX = coordinateX;
        containerBox.TranslationY = coordinateY;
        containerBox.ClassId = containerType;
        switch (containerBox.ClassId)
        {
            case "Pó³ka":
                containerBox.Color = Colors.BurlyWood; break;

            case "Lodówka":
                containerBox.Color = Colors.SkyBlue; break;
            case "Zamra¿arka":
                containerBox.Color = Colors.DeepSkyBlue; break;
            case "Stojak":
                containerBox.Color = Colors.SaddleBrown; break;
            case "Kasa":
                containerBox.Color = Colors.Gold; break;
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
    private async Task<Container> GetContainerFromDatabase(int containerId)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync($"{apiBaseUrl}/api/Containers/GetContainerById/{containerId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Container>(responseData);
            }
            else
            {
                throw new Exception("B³¹d podczas pobierania danych pojemnika.");
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
    private async void Back_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("EditProductContaiiners");
        await Navigation.PushAsync(ShopListPage);
    }
    private async void ShelfTapped(object sender, EventArgs e)
    {
        if (sender is BoxViewExtensions selectedShelf)
        {
            int containerId = BoxViewExtensions.GetId(selectedShelf);
            await Navigation.PushAsync(new ProductsInContainerPage(containerId, shopId, name));
        }
    }

}