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
                    // Operacja zako�czona powodzeniem
                }
                else
                {
                    // Obs�uga b��du
                    await DisplayAlert("B��d", "Nie uda�o si� zapisa� kontenera.", "OK");
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

                newShop.Name = await DisplayPromptAsync("Nowy sklep", "Podaj nazw� sklepu:");

                if (newShop.Name == null)
                {
                    return -1;
                }

                newShop.City = await DisplayPromptAsync("Nowy sklep", "Podaj miasto:");
                if (newShop.City == null) return -1;

                newShop.Street = await DisplayPromptAsync("Nowy sklep", "Podaj ulic�:");
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

                        // Teraz masz dost�p do shopId
                        var shopId = shop.ShopId;

                        return shopId;
                    }
                    else
                    {
                        // Obs�uga b��du
                        await DisplayAlert("B��d", "Nie uda�o si� zapisa� sklepu.", "OK");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Wyst�pi� b��d: {ex.Message}");
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
                await DisplayAlert("B��d", "Nie znaleziono sklepu o ClassId = 'Sklep'.", "OK");
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

        private async void MapCreate()
        {
            string dimensionsInput = await DisplayPromptAsync("Tworzenie sklepu", "Podaj wymiary (szeroko�� x wysoko��):");

            if (!string.IsNullOrEmpty(dimensionsInput))
            {
                string[] dimensions = dimensionsInput.Split('x');

                if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
                {
                    // Tworzenie prostok�ta
                    var Sklep = new BoxView
                    {
                        Color = Colors.LightGray, // Kolor prostok�ta
                        WidthRequest = szerokosc * skala, // Ustawienie szeroko�ci na podan� warto��
                        HeightRequest = wysokosc * skala // Ustawienie wysoko�ci na podan� warto��
                    };
                    Sklep.ClassId = "Sklep";
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += ShelfTapped;
                    Sklep.GestureRecognizers.Add(tapGesture);

                    // Dodanie prostok�ta do interfejsu u�ytkownika (np. do StackLayout lub Grid)
                    Layout.SetRow(Sklep, 1);
                    Layout.Children.Add(Sklep); // "MojeLayout" to kontener, do kt�rego chcemy doda� prostok�t.
                    // SaveContainerToDatabase(Sklep, szerokosc, wysokosc); // Ta funkcja zosta�a zakomentowana
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

        private void SwitchModeButton_Clicked(object sender, EventArgs e)
        {
            trybUsuwanie = !trybUsuwanie;
            string buttonText = trybUsuwanie ? "Prze��cz tryb Edycji" : "Prze��cz tryb usuwania";
            ((Button)sender).Text = buttonText;
        }

        private async void SaveMapButton_Clicked(object sender, EventArgs e)
        {
            SaveMapToDatabase();

            // Dodaj ewentualne dodatkowe dzia�ania po zapisaniu mapy
            // ...

        }
    }
}
