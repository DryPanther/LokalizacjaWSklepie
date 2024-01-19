using LokalizacjaWSklepie.Extensions;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace LokalizacjaWSklepie.Pages;

public partial class MapEditorPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private int skala = 100;
    private double skala1 = 100;
    private bool trybUsuwanie = false;
    private int shopId;
    private List<Container> existingContainerIds = new List<Container> { };

    public MapEditorPage(int shopId, string name)
    {
        Title = name;
        InitializeComponent();
        this.shopId = shopId;
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

                FrameExtensions.SetId(shopBox, shopMapData.ShopId);

                var containers = await GetContainersByShopIdFromDatabase(shopMapData.ShopId);

                foreach (var containerData in containers)
                {
                    var containerBox = CreateContainerBox(containerData.Width, containerData.Length, (int)Math.Round((double)containerData.CoordinateX *(skala1/100)), (int)Math.Round((double)containerData.CoordinateY*(skala1 / 100)), containerData.ContainerType);

                    FrameExtensions.SetId(containerBox, containerData.ContainerId);

                    Layout.Children.Add(containerBox);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas wczytywania mapy: {ex.Message}");
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
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += ShelfTapped;
        shopBox.GestureRecognizers.Add(tapGesture);
        Layout.SetRow(shopBox, 1);
        return shopBox;
    }

    private Frame CreateContainerBox(double width, double length, int coordinateX, int coordinateY, string containerType)
    {
        var containerBox = new FrameExtensions
        {
            WidthRequest = width * skala1,
            HeightRequest = length * skala1,
            CornerRadius = 10

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

        var przesunGestureRecognizer = new PanGestureRecognizer();
        przesunGestureRecognizer.PanUpdated += PrzesunProstokat;
        containerBox.GestureRecognizers.Add(przesunGestureRecognizer);

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
    private async Task SaveContainerChanges()
    {
        try
        {
            foreach (var child in Layout.Children)
            {
                if (child is FrameExtensions container && container.ClassId != "Sklep")
                {
                    int containerId = FrameExtensions.GetId(container);

                    if (containerId > 0)
                    {
                        var existingContainer = await GetContainerFromDatabase(containerId);

                        if (existingContainer != null)
                        {
                            existingContainer.ContainerId = containerId;
                            existingContainer.Width = container.Width / skala1;
                            existingContainer.Length = container.Height / skala1;
                            existingContainer.CoordinateX = (int)((container.TranslationX*100)/skala1);
                            existingContainer.CoordinateY = (int)((container.TranslationY * 100)/ skala1);
                            existingContainer.ContainerType = container.ClassId;

                            await UpdateContainerInDatabase(existingContainer);
                        }
                        existingContainerIds.Add(existingContainer);
                    }
                    else
                    {
                        await AddContainerToDatabase(container);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas zapisywania zmian w pojemnikach: {ex.Message}");
        }
    }

    private async Task UpdateContainerInDatabase(Container container)
    {
        using (HttpClient client = new HttpClient())
        {
            string json = JsonConvert.SerializeObject(container);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{apiBaseUrl}/api/Containers/UpdateContainer/{container.ContainerId}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("B³¹d podczas aktualizowania danych pojemnika.");
            }
        }
    }

    private async Task AddContainerToDatabase(FrameExtensions container)
    {
        using (HttpClient client = new HttpClient())
        {
            var newContainer = new Container
            {
                Width = container.Width / skala,
                Length = container.Height / skala,
                CoordinateX = (int)container.TranslationX,
                CoordinateY = (int)container.TranslationY,
                ShopId = shopId,
                ContainerType = container.ClassId
            };

            string json = JsonConvert.SerializeObject(newContainer);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiBaseUrl}/api/Containers/AddContainer", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);

            if (response.IsSuccessStatusCode)
            {
                var addedContainer = JsonConvert.DeserializeObject<Container>(await response.Content.ReadAsStringAsync());
                existingContainerIds.Add(addedContainer);
            }
            else
            {
                throw new Exception("B³¹d podczas dodawania nowego pojemnika.");
            }
        }
    }
    private async void ShelfTapped(object sender, EventArgs e)
    {
        if (sender is FrameExtensions selectedShelf)
        {
            if (!trybUsuwanie)
            {
                string dimensionsInput = await DisplayPromptAsync("Zmiana rozmiaru", "Podaj nowe wymiary (szerokoœæ x wysokoœæ):", "Ok", "Cancel", $"{selectedShelf.Width / skala}x{selectedShelf.Height / skala}");

                if (!string.IsNullOrEmpty(dimensionsInput))
                {
                    string[] dimensions = dimensionsInput.Split('x');

                    if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double newWidth) && double.TryParse(dimensions[1], out double newHeight))
                    {
                        selectedShelf.WidthRequest = newWidth * skala;
                        selectedShelf.HeightRequest = newHeight * skala;

                        if (selectedShelf.ClassId != "Sklep")
                        {
                            await ChangeContainerType(selectedShelf);
                            switch (selectedShelf.ClassId)
                            {
                                case "Pó³ka":
                                    selectedShelf.BackgroundColor = Colors.BurlyWood; break;

                                case "Lodówka":
                                    selectedShelf.BackgroundColor = Colors.SkyBlue; break;
                                case "Zamra¿arka":
                                    selectedShelf.BackgroundColor = Colors.DeepSkyBlue; break;
                                case "Stojak":
                                    selectedShelf.BackgroundColor = Colors.SaddleBrown; break;
                                case "Kasa":
                                    selectedShelf.BackgroundColor = Colors.Gold; break;
                                default:
                                    break;
                            }
                        }

                    }
                    else
                    {
                        await DisplayAlert("B³¹d", "WprowadŸ poprawne wartoœci liczbowe w formacie 'szerokoœæ x wysokoœæ'.", "OK");
                    }
                }
            }
            else if (trybUsuwanie)
            {
                if (selectedShelf.ClassId != "Sklep")
                {
                    Layout.Children.Remove(selectedShelf);
                }
            }
        }
    }
    private async Task ChangeContainerType(FrameExtensions selectedShelf)
    {
        List<string> availableTypes = new List<string> { "Pó³ka", "Lodówka", "Zamra¿arka", "Stojak", "Kasa" };
        string selectedType = await DisplayActionSheet("Wybierz typ pojemnika", "Anuluj", null, availableTypes.ToArray());

        if (selectedType != "Anuluj")
        {
            UpdateContainerTypeInMemory(selectedShelf, selectedType);
        }
    }
    private void UpdateContainerTypeInMemory(FrameExtensions selectedShelf, string newType)
    {
        selectedShelf.ClassId = newType;
    }
    private bool przesuwanie = false;
    private double poprzedniaX, poprzedniaY;
    private void PrzesunProstokat(object sender, PanUpdatedEventArgs e)
    {
        var prostokat = (FrameExtensions)sender;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                przesuwanie = true;
                poprzedniaX = prostokat.TranslationX;
                poprzedniaY = prostokat.TranslationY;
                break;

            case GestureStatus.Running:
                if (przesuwanie)
                {
                    prostokat.TranslationX = poprzedniaX + e.TotalX;
                    prostokat.TranslationY = poprzedniaY + e.TotalY;
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                przesuwanie = false;
                break;
        }
    }
    private void SwitchModeButton_Clicked(object sender, EventArgs e)
    {
        trybUsuwanie = !trybUsuwanie;
        string buttonText = trybUsuwanie ? "Prze³¹cz tryb Edycji" : "Prze³¹cz tryb usuwania";
        ((Button)sender).Text = buttonText;
    }
    private async Task UpdateShopInDatabase(Shop shop)
    {
        using (HttpClient client = new HttpClient())
        {
            string json = JsonConvert.SerializeObject(shop);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{apiBaseUrl}/api/Shops/UpdateShop/{shop.ShopId}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("B³¹d podczas aktualizowania danych sklepu.");
            }
        }
    }

    private async Task SaveShopChanges()
    {
        try
        {
            var shopBox = Layout.Children.FirstOrDefault(child => child is FrameExtensions box && box.ClassId == "Sklep") as FrameExtensions;

            if (shopBox != null)
            {
                int? shopId = GetShopIdFromMemory(shopBox);

                if (shopId.HasValue)
                {
                    var existingShop = await GetShopFromDatabase(shopId.Value);

                    if (existingShop != null)
                    {
                        existingShop.Width = shopBox.Width / skala1;
                        existingShop.Length = shopBox.Height / skala1;

                        await UpdateShopInDatabase(existingShop);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas zapisywania zmian w sklepie: {ex.Message}");
        }
    }
    private async Task<Shop> GetShopFromDatabase(int shopId)
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
                throw new Exception("B³¹d podczas pobierania danych sklepu.");
            }
        }
    }
    private async void SaveMapButton_Clicked(object sender, EventArgs e)
    {
        await SaveShopChanges();
        await SaveContainerChanges();
        await CheckAndHandleDeletedContainers(existingContainerIds);
        var ShopListPage = new ShopListPage("EditShops");
        await Navigation.PushAsync(ShopListPage);
    }
    private int? GetShopIdFromMemory(FrameExtensions shopBox)
    {
        return shopId;
    }
    private async void AddContainer_Clicked(object sender, EventArgs e)
    {
        string dimensionsResult = await DisplayPromptAsync("Dodawanie pojemnika", "Podaj wymiary (szerokoœæ x wysokoœæ):");

        if (!string.IsNullOrEmpty(dimensionsResult))
        {
            string[] dimensions = dimensionsResult.Split('x');

            if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
            {

                var prostokat = new FrameExtensions
                {
                    WidthRequest = szerokosc * skala,
                    HeightRequest = wysokosc * skala,
                    CornerRadius = 10

                };
                List<string> availableTypes = new List<string> { "Pó³ka", "Lodówka", "Zamra¿arka", "Stojak", "Kasa" };
                string selectedType = await DisplayActionSheet("Wybierz typ pojemnika", "Anuluj", null, availableTypes.ToArray());
                prostokat.ClassId = selectedType;
                switch (prostokat.ClassId)
                {
                    case "Pó³ka":
                        prostokat.BackgroundColor = Colors.BurlyWood; break;

                    case "Lodówka":
                        prostokat.BackgroundColor = Colors.SkyBlue; break;
                    case "Zamra¿arka":
                        prostokat.BackgroundColor = Colors.DeepSkyBlue; break;
                    case "Stojak":
                        prostokat.BackgroundColor = Colors.SaddleBrown; break;
                    case "Kasa":
                        prostokat.BackgroundColor = Colors.Gold; break;
                    default:
                        break;
                }
                var przesunGestureRecognizer = new PanGestureRecognizer();
                przesunGestureRecognizer.PanUpdated += PrzesunProstokat;
                prostokat.GestureRecognizers.Add(przesunGestureRecognizer);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += ShelfTapped;
                prostokat.GestureRecognizers.Add(tapGesture);

                Layout.SetRow(prostokat, 1);
                Layout.Children.Add(prostokat);
            }
            else
            {
                await DisplayAlert("B³¹d", "WprowadŸ poprawne wartoœci liczbowe w formacie 'szerokoœæ x wysokoœæ'.", "OK");
            }
        }
        else
        {
            await DisplayAlert("B³¹d", "WprowadŸ wymiary.", "OK");
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
    private async Task CheckAndHandleDeletedContainers(List<Container> existingContainerIds)
    {
        try
        {
            var containersInDatabase = await GetContainersByShopIdFromDatabase(shopId);
            var missingContainers = containersInDatabase
                .Where(dbContainer => existingContainerIds.All(uiContainer => uiContainer.ContainerId != dbContainer.ContainerId))
                .ToList();

            foreach (var missingContainer in missingContainers)
            {
                missingContainer.ShopId = null;
                missingContainer.CoordinateX = null;
                missingContainer.CoordinateY = null;

                await UpdateContainerInDatabase(missingContainer);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas aktualizowania brakuj¹cych pojemnika: {ex.Message}");
        }
    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        var ShopListPage = new ShopListPage("EditShops");
        await Navigation.PushAsync(ShopListPage);
    }
    private async void DeleteShopButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Deletion", "Are you sure you want to delete this shop and all its containers?", "Yes", "No");

        if (confirm)
        {
            try
            {
                await DeleteAllContainersForShopFromDatabase();
                await DeleteShopFromDatabase();
                var ShopListPage = new ShopListPage("EditShops");
                await Navigation.PushAsync(ShopListPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

    private async Task DeleteShopFromDatabase()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{apiBaseUrl}/api/Shops/DeleteShop/{shopId}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("B³¹d podczas usuwania sklepu z bazy danych.");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas usuwania sklepu z bazy danych: {ex.Message}");
        }
    }

    private async Task DeleteAllContainersForShopFromDatabase()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{apiBaseUrl}/api/Containers/DeleteAllContainersForShop/{shopId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    throw new Exception($"B³¹d podczas usuwania pojemnika sklepu z bazy danych. Kod odpowiedzi: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas usuwania pojemnika sklepu z bazy danych: {ex.Message}");
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