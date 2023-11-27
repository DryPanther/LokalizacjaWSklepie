using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;

namespace LokalizacjaWSklepie.Pages
{
    public partial class MapCreator : ContentPage
    {
        private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
        int skala = 100;
        bool trybUsuwanie = false;

        public MapCreator()
        {
            InitializeComponent();
            MapCreate();
        }

        private async Task SaveContainerToDatabase(BoxView container, double width, double height, int shopId)
        {
            var newContainer = new Container
            {
                ContainerType = container.ClassId,
                Width = width,
                Length = height,
                CoordinateX = (int)container.TranslationX,
                CoordinateY = (int)container.TranslationY,
                ShopId = shopId
            };

            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(newContainer);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiBaseUrl}/api/Containers/AddContainer", content);

                if (response.IsSuccessStatusCode)
                {
                    // Operacja zakoñczona powodzeniem
                }
                else
                {
                    // Obs³uga b³êdu
                    await DisplayAlert("B³¹d", "Nie uda³o siê zapisaæ kontenera.", "OK");
                }
            }
        }

        private async Task<int> SaveShopToDatabase(BoxView shopBox)
        {
            try
            {
                var newShop = new Shop
                {
                    Width = shopBox.Width / skala,
                    Length = shopBox.Height / skala
                };

                newShop.Name = await DisplayPromptAsync("Nowy sklep", "Podaj nazwê sklepu:");

                if (newShop.Name == null)
                {
                    return -1;
                }

                newShop.City = await DisplayPromptAsync("Nowy sklep", "Podaj miasto:");
                if (newShop.City == null) return -1;

                newShop.Street = await DisplayPromptAsync("Nowy sklep", "Podaj ulicê:");
                if (newShop.Street == null) return -1;

                newShop.PostalCode = await DisplayPromptAsync("Nowy sklep", "Podaj kod pocztowy:");
                if (newShop.PostalCode == null) return -1;

                using (HttpClient client = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(newShop);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{apiBaseUrl}/api/Shops/AddShop", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var shop = JsonConvert.DeserializeObject<Shop>(responseData);

                        // Teraz masz dostêp do shopId
                        var shopId = shop.ShopId;

                        return shopId;
                    }
                    else
                    {
                        // Obs³uga b³êdu
                        await DisplayAlert("B³¹d", "Nie uda³o siê zapisaæ sklepu.", "OK");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Wyst¹pi³ b³¹d: {ex.Message}");
                return -1;
            }
        }
        private async Task SaveMapToDatabase()
        {
            var shopBox = Layout.Children.FirstOrDefault(child => child is BoxView box && box.ClassId == "Sklep") as BoxView;

            if (shopBox != null)
            {
                int shopId = await SaveShopToDatabase(shopBox);

                foreach (var child in Layout.Children)
                {
                    if (child is BoxView container && container.ClassId != "Sklep")
                    {
                        await SaveContainerToDatabase(container, container.Width / skala, container.Height / skala, shopId);
                    }
                }

                Page targetPage = new MainPage();
                await Navigation.PushAsync(targetPage);
            }
            else
            {
                await DisplayAlert("B³¹d", "Nie znaleziono sklepu o ClassId = 'Sklep'.", "OK");
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

        private async void MapCreate()
        {
            string dimensionsInput = await DisplayPromptAsync("Tworzenie sklepu", "Podaj wymiary (szerokoœæ x wysokoœæ):");

            if (!string.IsNullOrEmpty(dimensionsInput))
            {
                string[] dimensions = dimensionsInput.Split('x');

                if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
                {
                    // Tworzenie prostok¹ta
                    var Sklep = new BoxView
                    {
                        Color = Colors.LightGray, // Kolor prostok¹ta
                        WidthRequest = szerokosc * skala, // Ustawienie szerokoœci na podan¹ wartoœæ
                        HeightRequest = wysokosc * skala // Ustawienie wysokoœci na podan¹ wartoœæ
                    };
                    Sklep.ClassId = "Sklep";
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += ShelfTapped;
                    Sklep.GestureRecognizers.Add(tapGesture);

                    // Dodanie prostok¹ta do interfejsu u¿ytkownika (np. do StackLayout lub Grid)
                    Layout.SetRow(Sklep, 1);
                    Layout.Children.Add(Sklep); // "MojeLayout" to kontener, do którego chcemy dodaæ prostok¹t.
                    // SaveContainerToDatabase(Sklep, szerokosc, wysokosc); // Ta funkcja zosta³a zakomentowana
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

        private void SwitchModeButton_Clicked(object sender, EventArgs e)
        {
            trybUsuwanie = !trybUsuwanie;
            string buttonText = trybUsuwanie ? "Prze³¹cz tryb Edycji" : "Prze³¹cz tryb usuwania";
            ((Button)sender).Text = buttonText;
        }

        private async void SaveMapButton_Clicked(object sender, EventArgs e)
        {
            SaveMapToDatabase();

            // Dodaj ewentualne dodatkowe dzia³ania po zapisaniu mapy
            // ...

        }
    }
}
