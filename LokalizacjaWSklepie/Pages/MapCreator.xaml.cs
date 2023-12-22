using LokalizacjaWSklepie.Extensions;
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

        private async Task SaveContainerToDatabase(BoxViewExtensions container, double width, double height, int shopId)
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
                }
                else
                {
                    await DisplayAlert("B��d", "Nie uda�o si� zapisa� pojemnika.", "OK");
                }
            }
        }

        private async Task<int> SaveShopToDatabase(BoxViewExtensions shopBox)
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

                        var shopId = shop.ShopId;

                        return shopId;
                    }
                    else
                    {
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
            var shopBox = Layout.Children.FirstOrDefault(child => child is BoxViewExtensions box && box.ClassId == "Sklep") as BoxViewExtensions;

            if (shopBox != null)
            {
                int shopId = await SaveShopToDatabase(shopBox);

                foreach (var child in Layout.Children)
                {
                    if (child is BoxViewExtensions container && container.ClassId != "Sklep")
                    {
                        await SaveContainerToDatabase(container, container.Width / skala, container.Height / skala, shopId);
                    }
                }

                Page AdminMenuPage = new AdminMenuPage();
                await Navigation.PushAsync(AdminMenuPage);
            }
            else
            {
                await DisplayAlert("B��d", "Nie znaleziono sklepu o ClassId = 'Sklep'.", "OK");
            }
        }
        private async void ShelfTapped(object sender, EventArgs e)
        {
            if (sender is BoxViewExtensions selectedShelf)
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

                            if (selectedShelf.ClassId != "Sklep")
                            {
                                await ChangeContainerType(selectedShelf);
                                switch (selectedShelf.ClassId)
                                {
                                    case "P�ka":
                                        selectedShelf.BackgroundColor = Colors.BurlyWood; break;

                                    case "Lod�wka":
                                        selectedShelf.BackgroundColor = Colors.SkyBlue; break;
                                    case "Zamra�arka":
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
                            await DisplayAlert("B��d", "Wprowad� poprawne warto�ci liczbowe w formacie 'szeroko�� x wysoko��'.", "OK");
                        }
                    }
                }
                else if (trybUsuwanie)
                {
                    if (selectedShelf.ClassId != "Sklep")
                    {
                        bool shouldDelete = await DisplayAlert("Potwierd� usuni�cie", "Czy na pewno chcesz usun�� ten pojemnik?", "Tak", "Anuluj");

                        if (shouldDelete)
                        {
                            Layout.Children.Remove(selectedShelf);
                        }
                    }
                }
            }
        }

        private async Task ChangeContainerType(BoxViewExtensions selectedShelf)
        {
            List<string> availableTypes = new List<string> { "P�ka", "Lod�wka", "Zamra�arka", "Stojak", "Kasa" };
            string selectedType = await DisplayActionSheet("Wybierz typ pojemnika", "Anuluj", null, availableTypes.ToArray());

            if (selectedType != "Anuluj")
            {
                UpdateContainerTypeInMemory(selectedShelf, selectedType);
            }
        }

        private void UpdateContainerTypeInMemory(BoxViewExtensions selectedShelf, string newType)
        {
            selectedShelf.ClassId = newType;
        }

        private async void MapCreate()
        {
            string dimensionsInput = await DisplayPromptAsync("Tworzenie sklepu", "Podaj wymiary (szeroko�� x wysoko��):");
            if (dimensionsInput == null)
            {
                await Navigation.PopAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(dimensionsInput))
                {
                    string[] dimensions = dimensionsInput.Split('x');

                    if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
                    {
                        var Sklep = new BoxViewExtensions
                        {
                            BackgroundColor = Colors.LightGray,
                            WidthRequest = szerokosc * skala,
                            HeightRequest = wysokosc * skala
                        };
                        Sklep.ClassId = "Sklep";
                        var tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += ShelfTapped;
                        Sklep.GestureRecognizers.Add(tapGesture);

                        Layout.SetRow(Sklep, 1);
                        Layout.Children.Add(Sklep);
                    }
                    else
                    {
                        await DisplayAlert("B��d", "Wprowad� poprawne warto�ci liczbowe w formacie 'szeroko�� x wysoko��'.", "OK");
                        MapCreate();
                    }
                }
                else
                {
                    await DisplayAlert("B��d", "Wprowad� wymiary.", "OK");
                    MapCreate();
                }
            }

        }


        private bool przesuwanie = false;
        private double poprzedniaX, poprzedniaY;

        private void PrzesunProstokat(object sender, PanUpdatedEventArgs e)
        {
            var prostokat = (BoxViewExtensions)sender;

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

        private async void AddContainer_Clicked(object sender, EventArgs e)
        {
            string dimensionsResult = await DisplayPromptAsync("Dodawanie pojemnika", "Podaj wymiary (szeroko�� x wysoko��):");

            if (!string.IsNullOrEmpty(dimensionsResult))
            {
                string[] dimensions = dimensionsResult.Split('x');

                if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
                {
                    var prostokat = new BoxViewExtensions
                    {
                        WidthRequest = szerokosc * skala,
                        HeightRequest = wysokosc * skala,
                        CornerRadius = 10

                    };
                    List<string> availableTypes = new List<string> { "P�ka", "Lod�wka", "Zamra�arka", "Stojak", "Kasa" };
                    string selectedType = await DisplayActionSheet("Wybierz typ pojemnika", "Anuluj", null, availableTypes.ToArray());
                    if (string.IsNullOrEmpty(selectedType))
                    {
                        return;
                    }
                    prostokat.ClassId = selectedType;
                    switch (prostokat.ClassId)
                    {
                        case "P�ka":
                            prostokat.BackgroundColor = Colors.BurlyWood; break;

                        case "Lod�wka":
                            prostokat.BackgroundColor = Colors.SkyBlue; break;
                        case "Zamra�arka":
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
                    await DisplayAlert("B��d", "Wprowad� poprawne warto�ci liczbowe w formacie 'szeroko�� x wysoko��'.", "OK");
                }
            }
            else
            {
                await DisplayAlert("B��d", "Wprowad� wymiary.", "OK");
            }
        }

        private void SwitchModeButton_Clicked(object sender, EventArgs e)
        {
            trybUsuwanie = !trybUsuwanie;
            string buttonText = trybUsuwanie ? "Prze��cz tryb Edycji" : "Prze��cz tryb usuwania";
            ((Button)sender).Text = buttonText;
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            var ShopListPage = new ShopListPage("EditShops");
            await Navigation.PushAsync(ShopListPage);
        }

        private async void SaveMapButton_Clicked(object sender, EventArgs e)
        {
            await SaveMapToDatabase();
        }
    }
}
