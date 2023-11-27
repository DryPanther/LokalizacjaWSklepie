using LokalizacjaWSklepie.Extensions;
using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;

namespace LokalizacjaWSklepie.Pages;

public partial class MapEditorPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private int skala = 100;
    private bool trybUsuwanie = false;
    private int shopId;

    public MapEditorPage(int shopId)
    {
        InitializeComponent();
        this.shopId = shopId;
        LoadMap();
    }

    private async Task LoadMap()
    {
        try
        {
            // Tutaj umie�� kod wczytuj�cy map�
            // Zak�adam, �e masz dost�p do danych mapy w formie obiektu ShopMapData
            var shopMapData = await GetShopMapDataFromDatabase();

            if (shopMapData != null)
            {
                // Tworzenie prostok�ta reprezentuj�cego sklep
                var shopBox = CreateShopBox(shopMapData.Width, shopMapData.Length);
                Layout.Children.Add(shopBox);

                // Przypisanie Id z bazy danych do sklepu
                BoxViewExtensions.SetId(shopBox, shopMapData.ShopId);

                // Pobierz wszystkie kontenery z bazy danych dla tego sklepu
                var containers = await GetContainersByShopIdFromDatabase(shopMapData.ShopId);

                // Dodawanie kontener�w na podstawie danych z bazy danych
                foreach (var containerData in containers)
                {
                    var containerBox = CreateContainerBox(containerData.Width, containerData.Length, (int)containerData.CoordinateX, (int)containerData.CoordinateY);

                    // Przypisanie Id z bazy danych do kontenera
                    BoxViewExtensions.SetId(containerBox, containerData.ContainerId);

                    Layout.Children.Add(containerBox);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B��d podczas wczytywania mapy: {ex.Message}");
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
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += ShelfTapped;
        shopBox.GestureRecognizers.Add(tapGesture);
        Layout.SetRow(shopBox, 1);
        return shopBox;
    }

    private BoxView CreateContainerBox(double width, double length, int coordinateX, int coordinateY)
    {
        var containerBox = new BoxView
        {
            WidthRequest = width * skala,
            HeightRequest = length * skala,
            CornerRadius = new CornerRadius(10)
        };

        // Ustawienie wsp�rz�dnych kontenera
        containerBox.TranslationX = coordinateX;
        containerBox.TranslationY = coordinateY;

        // Dodanie obs�ugi zdarzenia klikni�cia na kontener
        var przesunGestureRecognizer = new PanGestureRecognizer();
        przesunGestureRecognizer.PanUpdated += PrzesunProstokat;
        containerBox.GestureRecognizers.Add(przesunGestureRecognizer);

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += ShelfTapped;
        containerBox.GestureRecognizers.Add(tapGesture);

        return containerBox;
    }

    private async Task<Shop> GetShopMapDataFromDatabase()
    {
        using (HttpClient client = new HttpClient())
        {
            // Pobierz dane mapy dla konkretnego sklepu
            var response = await client.GetAsync($"{apiBaseUrl}/api/Shops/GetShopById/{shopId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Shop>(responseData);
            }
            else
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas pobierania danych mapy ze sklepu.");
            }
        }
    }
    private async Task<Container> GetContainerFromDatabase(int containerId)
    {
        using (HttpClient client = new HttpClient())
        {
            // Pobierz dane kontenera
            var response = await client.GetAsync($"{apiBaseUrl}/api/Containers/{containerId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Container>(responseData);
            }
            else
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas pobierania danych kontenera.");
            }
        }
    }
    private async Task SaveContainerChanges()
    {
        try
        {
            foreach (var child in Layout.Children)
            {
                if (child is BoxViewExtensions container && container.ClassId != "Sklep")
                {
                    int containerId = BoxViewExtensions.GetId(container);

                    if (containerId > 0)
                    {
                        var existingContainer = await GetContainerFromDatabase(containerId);

                        if (existingContainer != null)
                        {
                            existingContainer.Width = container.Width / skala;
                            existingContainer.Length = container.Height / skala;
                            existingContainer.CoordinateX = (int)container.TranslationX;
                            existingContainer.CoordinateY = (int)container.TranslationY;

                            await UpdateContainerInDatabase(existingContainer);
                        }
                    }
                    else
                    {
                        // Je�li ContainerId nie istnieje, to oznacza, �e kontener zosta� dodany w trakcie edycji mapy
                        // Dodaj nowy kontener do bazy danych
                        await AddContainerToDatabase(container);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B��d podczas zapisywania zmian w kontenerach: {ex.Message}");
        }
    }

    private async Task UpdateContainerInDatabase(Container container)
    {
        using (HttpClient client = new HttpClient())
        {
            // Aktualizuj dane kontenera
            string json = JsonConvert.SerializeObject(container);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{apiBaseUrl}/api/Containers/UpdateContainer/{container.ContainerId}", content);

            if (!response.IsSuccessStatusCode)
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas aktualizowania danych kontenera.");
            }
        }
    }

    private async Task AddContainerToDatabase(BoxView container)
    {
        using (HttpClient client = new HttpClient())
        {
            // Dodaj nowy kontener
            var newContainer = new Container
            {
                Width = container.Width / skala,
                Length = container.Height / skala,
                CoordinateX = (int)container.TranslationX,
                CoordinateY = (int)container.TranslationY,
                ShopId = shopId
            };

            string json = JsonConvert.SerializeObject(newContainer);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiBaseUrl}/api/Containers/AddContainer", content);

            if (!response.IsSuccessStatusCode)
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas dodawania nowego kontenera.");
            }
        }
    }
    private async void ShelfTapped(object sender, EventArgs e)
    {
        if (sender is BoxView selectedShelf)
        {
            if (!trybUsuwanie)
            {
                string dimensionsInput = await DisplayPromptAsync("Zmiana rozmiaru", "Podaj nowe wymiary (szeroko�� x wysoko��):", "Ok", "Cancel", $"{selectedShelf.Width / skala}x{selectedShelf.Height / skala}");

                if (!string.IsNullOrEmpty(dimensionsInput))
                {
                    string[] dimensions = dimensionsInput.Split('x');

                    if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double newWidth) && double.TryParse(dimensions[1], out double newHeight))
                    {
                        selectedShelf.WidthRequest = newWidth * skala;
                        selectedShelf.HeightRequest = newHeight * skala;

                        // Kod do zmiany typu kontenera zosta� tutaj pozostawiony
                        if (selectedShelf.ClassId != "Sklep")
                        {
                            await ChangeContainerType(selectedShelf);
                            switch (selectedShelf.ClassId)
                            {
                                case "P�ka":
                                    selectedShelf.Color = Colors.BurlyWood; break;

                                case "Lod�wka":
                                    selectedShelf.Color = Colors.SkyBlue; break;
                                case "Zamra�arka":
                                    selectedShelf.Color = Colors.DeepSkyBlue; break;
                                case "Stojak":
                                    selectedShelf.Color = Colors.SaddleBrown; break;
                                case "Kasa":
                                    selectedShelf.Color = Colors.Gold; break;
                                default:
                                    break;
                            }
                        }

                    }
                    else
                    {
                        await DisplayAlert("B��d", "Wprowad� poprawne warto�ci liczbowe w formacie 'szeroko�� x wysoko��'.", "OK");
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
    private async Task ChangeContainerType(BoxView selectedShelf)
    {
        // Kod do zmiany typu kontenera zosta� tutaj pozostawiony
        List<string> availableTypes = new List<string> { "P�ka", "Lod�wka", "Zamra�arka", "Stojak", "Kasa" };
        string selectedType = await DisplayActionSheet("Wybierz typ kontenera", "Anuluj", null, availableTypes.ToArray());

        if (selectedType != "Anuluj")
        {
            UpdateContainerTypeInMemory(selectedShelf, selectedType);
        }
    }
    private void UpdateContainerTypeInMemory(BoxView selectedShelf, string newType)
    {
        // Zaktualizuj typ kontenera bez zapisywania do bazy danych
        // Ta funkcja po prostu aktualizuje typ kontenera w pami�ci, bez operacji bazodanowych
        selectedShelf.ClassId = newType; // W tym przyk�adzie u�ywam ClassId do przechowywania typu kontenera
    }
    private bool przesuwanie = false;
    private double poprzedniaX, poprzedniaY;
    private void PrzesunProstokat(object sender, PanUpdatedEventArgs e)
    {
        var prostokat = (BoxView)sender;

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
        string buttonText = trybUsuwanie ? "Prze��cz tryb Edycji" : "Prze��cz tryb usuwania";
        ((Button)sender).Text = buttonText;
    }
    private async Task UpdateShopInDatabase(Shop shop)
    {
        using (HttpClient client = new HttpClient())
        {
            // Aktualizuj dane sklepu
            string json = JsonConvert.SerializeObject(shop);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{apiBaseUrl}/api/Shops/UpdateShop/{shop.ShopId}", content);

            if (!response.IsSuccessStatusCode)
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas aktualizowania danych sklepu.");
            }
        }
    }

    private async Task SaveShopChanges()
    {
        try
        {
            // Pobierz obiekt BoxView reprezentuj�cy sklep
            var shopBox = Layout.Children.FirstOrDefault(child => child is BoxView box && box.ClassId == "Sklep") as BoxView;

            if (shopBox != null)
            {
                // Pobierz Id sklepu z pami�ci
                int? shopId = GetShopIdFromMemory(shopBox);

                if (shopId.HasValue)
                {
                    // Pobierz dane sklepu z bazy danych
                    var existingShop = await GetShopFromDatabase(shopId.Value);

                    if (existingShop != null)
                    {
                        // Aktualizuj szeroko�� i d�ugo�� sklepu
                        existingShop.Width = shopBox.Width / skala;
                        existingShop.Length = shopBox.Height / skala;

                        // Aktualizuj dane sklepu w bazie danych
                        await UpdateShopInDatabase(existingShop);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B��d podczas zapisywania zmian w sklepie: {ex.Message}");
        }
    }
    private async Task<Shop> GetShopFromDatabase(int shopId)
    {
        using (HttpClient client = new HttpClient())
        {
            // Pobierz dane sklepu
            var response = await client.GetAsync($"{apiBaseUrl}/api/Shops/GetShopById/{shopId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Shop>(responseData);
            }
            else
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas pobierania danych sklepu.");
            }
        }
    }
    private async void SaveMapButton_Clicked(object sender, EventArgs e)
    {
        SaveShopChanges();
        SaveContainerChanges();

        // Dodaj ewentualne dodatkowe dzia�ania po zapisaniu mapy
        // ...

    }
    private int? GetShopIdFromMemory(BoxView shopBox)
    {
        return shopId;
    }
    private async void AddRectangle_Clicked(object sender, EventArgs e)
    {
        string dimensionsResult = await DisplayPromptAsync("Dodawanie pojemnika", "Podaj wymiary (szeroko�� x wysoko��):");

        if (!string.IsNullOrEmpty(dimensionsResult))
        {
            string[] dimensions = dimensionsResult.Split('x');

            if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
            {

                // Tworzenie prostok�ta
                var prostokat = new BoxView
                {
                    WidthRequest = szerokosc * skala, // Ustawienie szeroko�ci na podan� warto��
                    HeightRequest = wysokosc * skala, // Ustawienie wysoko�ci na podan� warto��
                    CornerRadius = new CornerRadius(10)

                };
                List<string> availableTypes = new List<string> { "P�ka", "Lod�wka", "Zamra�arka", "Stojak", "Kasa" };
                string selectedType = await DisplayActionSheet("Wybierz typ kontenera", "Anuluj", null, availableTypes.ToArray());
                prostokat.ClassId = selectedType;
                switch (prostokat.ClassId)
                {
                    case "P�ka":
                        prostokat.Color = Colors.BurlyWood; break;

                    case "Lod�wka":
                        prostokat.Color = Colors.SkyBlue; break;
                    case "Zamra�arka":
                        prostokat.Color = Colors.DeepSkyBlue; break;
                    case "Stojak":
                        prostokat.Color = Colors.SaddleBrown; break;
                    case "Kasa":
                        prostokat.Color = Colors.Gold; break;
                    default:
                        break;
                }
                // Dodanie obs�ugi zdarzenia klikni�cia na prostok�t
                // Dodajemy obs�ug� przesuwania
                var przesunGestureRecognizer = new PanGestureRecognizer();
                przesunGestureRecognizer.PanUpdated += PrzesunProstokat;
                prostokat.GestureRecognizers.Add(przesunGestureRecognizer);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += ShelfTapped;
                prostokat.GestureRecognizers.Add(tapGesture);

                // Dodanie prostok�ta do interfejsu u�ytkownika (np. do StackLayout lub Grid)
                Layout.SetRow(prostokat, 1);
                Layout.Children.Add(prostokat); // "MojeLayout" to kontener, do kt�rego chcemy doda� prostok�t.
                                                // SaveContainerToDatabase(prostokat, szerokosc, wysokosc, dbContext.Shops.Find(1)); // Ta funkcja zosta�a zakomentowana
            }
            else
            {
                await DisplayAlert("B��d", "Wprowad� poprawne warto�ci liczbowe w formacie 'szeroko�� x wysoko��'.", "OK");
            }
        }
        else
        {
            // Obs�uga przypadku, gdy u�ytkownik nie wprowadzi� �adnych wymiar�w.
            await DisplayAlert("B��d", "Wprowad� wymiary.", "OK");
        }
    }
    private async Task<List<Container>> GetContainersByShopIdFromDatabase(int shopId)
    {
        using (HttpClient client = new HttpClient())
        {
            // Pobierz wszystkie kontenery dla danego sklepu
            var response = await client.GetAsync($"{apiBaseUrl}/api/Containers/GetContainersByShopId/{shopId}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Container>>(responseData);
            }
            else
            {
                // Obs�uga b��du
                throw new Exception("B��d podczas pobierania kontener�w z bazy danych.");
            }
        }
    }
}