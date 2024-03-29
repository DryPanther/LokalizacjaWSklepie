using LokalizacjaWSklepie.Extensions;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;

namespace LokalizacjaWSklepie.Pages;

public partial class EditProductContainersPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private int skala = 100;
    private double skala1 = 100;
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
                shopBox.ClassId = "Sklep";
                Layout.Children.Add(shopBox);

                FrameExtensions.SetId(shopBox, shopMapData.ShopId);

                var containers = await GetContainersByShopIdFromDatabase(shopMapData.ShopId);

                foreach (var containerData in containers)
                {
                    var containerBox = CreateContainerBox(containerData.Width, containerData.Length, (int)containerData.CoordinateX, (int)containerData.CoordinateY, containerData.ContainerType);

                    FrameExtensions.SetId(containerBox, containerData.ContainerId);

                    Layout.Children.Add(containerBox);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B��d podczas wczytywania mapy: {ex.Message}");
        }
    }

    private FrameExtensions CreateShopBox(double width, double length)
    {
        var shopBox = new FrameExtensions
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
        var containerBox = new FrameExtensions
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
            case "P�ka":
                containerBox.BackgroundColor = Colors.BurlyWood; break;

            case "Lod�wka":
                containerBox.BackgroundColor = Colors.SkyBlue; break;
            case "Zamra�arka":
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
                throw new Exception("B��d podczas pobierania danych mapy ze sklepu.");
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
                throw new Exception("B��d podczas pobierania pojemnika z bazy danych.");
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
        if (sender is FrameExtensions selectedShelf)
        {
            int containerId = FrameExtensions.GetId(selectedShelf);
            await Navigation.PushAsync(new ProductsInContainerPage(containerId, shopId, name));
        }
    }
    private void ScaleAdd_Clicked(object sender, EventArgs e)
    {
        if (skala1 < 150)
        {
            foreach (var item in Layout.OfType<FrameExtensions>())
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
            foreach (var item in Layout.OfType<FrameExtensions>())
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