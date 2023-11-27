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
            // Tutaj umieœæ kod wczytuj¹cy mapê
            // Zak³adam, ¿e masz dostêp do danych mapy w formie obiektu ShopMapData
            var shopMapData = await GetShopMapDataFromDatabase();

            if (shopMapData != null)
            {
                // Tworzenie prostok¹ta reprezentuj¹cego sklep
                var shopBox = CreateShopBox(shopMapData.Width, shopMapData.Length);
                Layout.Children.Add(shopBox);

                // Przypisanie Id z bazy danych do sklepu
                BoxViewExtensions.SetId(shopBox, shopMapData.ShopId);

                // Pobierz wszystkie kontenery z bazy danych dla tego sklepu
                var containers = await GetContainersByShopIdFromDatabase(shopMapData.ShopId);

                // Dodawanie kontenerów na podstawie danych z bazy danych
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

        // Ustawienie wspó³rzêdnych kontenera
        containerBox.TranslationX = coordinateX;
        containerBox.TranslationY = coordinateY;

        // Dodanie obs³ugi zdarzenia klikniêcia na kontener
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas pobierania danych mapy ze sklepu.");
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas pobierania danych kontenera.");
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
                        // Jeœli ContainerId nie istnieje, to oznacza, ¿e kontener zosta³ dodany w trakcie edycji mapy
                        // Dodaj nowy kontener do bazy danych
                        await AddContainerToDatabase(container);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"B³¹d podczas zapisywania zmian w kontenerach: {ex.Message}");
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas aktualizowania danych kontenera.");
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas dodawania nowego kontenera.");
            }
        }
    }
    private async void ShelfTapped(object sender, EventArgs e)
    {
        if (sender is BoxView selectedShelf)
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

                        // Kod do zmiany typu kontenera zosta³ tutaj pozostawiony
                        if (selectedShelf.ClassId != "Sklep")
                        {
                            await ChangeContainerType(selectedShelf);
                            switch (selectedShelf.ClassId)
                            {
                                case "Pó³ka":
                                    selectedShelf.Color = Colors.BurlyWood; break;

                                case "Lodówka":
                                    selectedShelf.Color = Colors.SkyBlue; break;
                                case "Zamra¿arka":
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
    private async Task ChangeContainerType(BoxView selectedShelf)
    {
        // Kod do zmiany typu kontenera zosta³ tutaj pozostawiony
        List<string> availableTypes = new List<string> { "Pó³ka", "Lodówka", "Zamra¿arka", "Stojak", "Kasa" };
        string selectedType = await DisplayActionSheet("Wybierz typ kontenera", "Anuluj", null, availableTypes.ToArray());

        if (selectedType != "Anuluj")
        {
            UpdateContainerTypeInMemory(selectedShelf, selectedType);
        }
    }
    private void UpdateContainerTypeInMemory(BoxView selectedShelf, string newType)
    {
        // Zaktualizuj typ kontenera bez zapisywania do bazy danych
        // Ta funkcja po prostu aktualizuje typ kontenera w pamiêci, bez operacji bazodanowych
        selectedShelf.ClassId = newType; // W tym przyk³adzie u¿ywam ClassId do przechowywania typu kontenera
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
        string buttonText = trybUsuwanie ? "Prze³¹cz tryb Edycji" : "Prze³¹cz tryb usuwania";
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas aktualizowania danych sklepu.");
            }
        }
    }

    private async Task SaveShopChanges()
    {
        try
        {
            // Pobierz obiekt BoxView reprezentuj¹cy sklep
            var shopBox = Layout.Children.FirstOrDefault(child => child is BoxView box && box.ClassId == "Sklep") as BoxView;

            if (shopBox != null)
            {
                // Pobierz Id sklepu z pamiêci
                int? shopId = GetShopIdFromMemory(shopBox);

                if (shopId.HasValue)
                {
                    // Pobierz dane sklepu z bazy danych
                    var existingShop = await GetShopFromDatabase(shopId.Value);

                    if (existingShop != null)
                    {
                        // Aktualizuj szerokoœæ i d³ugoœæ sklepu
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
            throw new Exception($"B³¹d podczas zapisywania zmian w sklepie: {ex.Message}");
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas pobierania danych sklepu.");
            }
        }
    }
    private async void SaveMapButton_Clicked(object sender, EventArgs e)
    {
        SaveShopChanges();
        SaveContainerChanges();

        // Dodaj ewentualne dodatkowe dzia³ania po zapisaniu mapy
        // ...

    }
    private int? GetShopIdFromMemory(BoxView shopBox)
    {
        return shopId;
    }
    private async void AddRectangle_Clicked(object sender, EventArgs e)
    {
        string dimensionsResult = await DisplayPromptAsync("Dodawanie pojemnika", "Podaj wymiary (szerokoœæ x wysokoœæ):");

        if (!string.IsNullOrEmpty(dimensionsResult))
        {
            string[] dimensions = dimensionsResult.Split('x');

            if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
            {

                // Tworzenie prostok¹ta
                var prostokat = new BoxView
                {
                    WidthRequest = szerokosc * skala, // Ustawienie szerokoœci na podan¹ wartoœæ
                    HeightRequest = wysokosc * skala, // Ustawienie wysokoœci na podan¹ wartoœæ
                    CornerRadius = new CornerRadius(10)

                };
                List<string> availableTypes = new List<string> { "Pó³ka", "Lodówka", "Zamra¿arka", "Stojak", "Kasa" };
                string selectedType = await DisplayActionSheet("Wybierz typ kontenera", "Anuluj", null, availableTypes.ToArray());
                prostokat.ClassId = selectedType;
                switch (prostokat.ClassId)
                {
                    case "Pó³ka":
                        prostokat.Color = Colors.BurlyWood; break;

                    case "Lodówka":
                        prostokat.Color = Colors.SkyBlue; break;
                    case "Zamra¿arka":
                        prostokat.Color = Colors.DeepSkyBlue; break;
                    case "Stojak":
                        prostokat.Color = Colors.SaddleBrown; break;
                    case "Kasa":
                        prostokat.Color = Colors.Gold; break;
                    default:
                        break;
                }
                // Dodanie obs³ugi zdarzenia klikniêcia na prostok¹t
                // Dodajemy obs³ugê przesuwania
                var przesunGestureRecognizer = new PanGestureRecognizer();
                przesunGestureRecognizer.PanUpdated += PrzesunProstokat;
                prostokat.GestureRecognizers.Add(przesunGestureRecognizer);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += ShelfTapped;
                prostokat.GestureRecognizers.Add(tapGesture);

                // Dodanie prostok¹ta do interfejsu u¿ytkownika (np. do StackLayout lub Grid)
                Layout.SetRow(prostokat, 1);
                Layout.Children.Add(prostokat); // "MojeLayout" to kontener, do którego chcemy dodaæ prostok¹t.
                                                // SaveContainerToDatabase(prostokat, szerokosc, wysokosc, dbContext.Shops.Find(1)); // Ta funkcja zosta³a zakomentowana
            }
            else
            {
                await DisplayAlert("B³¹d", "WprowadŸ poprawne wartoœci liczbowe w formacie 'szerokoœæ x wysokoœæ'.", "OK");
            }
        }
        else
        {
            // Obs³uga przypadku, gdy u¿ytkownik nie wprowadzi³ ¿adnych wymiarów.
            await DisplayAlert("B³¹d", "WprowadŸ wymiary.", "OK");
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
                // Obs³uga b³êdu
                throw new Exception("B³¹d podczas pobierania kontenerów z bazy danych.");
            }
        }
    }
}