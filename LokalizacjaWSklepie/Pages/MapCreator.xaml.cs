using LokalizacjaWSklepie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LokalizacjaWSklepie.Pages
{
    public partial class MapCreator : ContentPage
    {
        int skala = 100;
        bool trybUsuwanie = false;
        private LokalizacjaWsklepieContext dbContext = new LokalizacjaWsklepieContext();
        public MapCreator()
        {
            InitializeComponent();
            MapCreate();
        }

        private void SaveContainerToDatabase(BoxView container, double width, double height, int shopId)
        {
            // Utw�rz nowy kontener na podstawie danych wprowadzonych przez u�ytkownika
            var newContainer = new Container
            {
                ContainerType = "Typ Kontenera", // Okre�l odpowiedni typ kontenera
                Width = width,
                Length = height,
                CoordinateX = (int)container.TranslationX, // Konwertuj do int, aby pasowa�o do modelu danych
                CoordinateY = (int)container.TranslationY,
                ShopId = shopId // Przypisz ShopId do kontenera
            };

            // Dodaj kontener do bazy danych
            dbContext.Containers.Add(newContainer);
        }

        private async void SaveMapToDatabase()
        {
            // Pobierz sklep z Layoutu
            var shopBox = Layout.Children.FirstOrDefault(child => child is BoxView box && box.AutomationId == "Sklep") as BoxView;

            if (shopBox != null)
            {
                // Zapisz sklep do bazy danych i pobierz nadane ShopId
                int shopId = await SaveShopToDatabase(shopBox);
                // Iteruj przez wszystkie elementy w Layout
                foreach (var child in Layout.Children)
                {
                    if (child is BoxView container)
                    {
                        // Sprawd�, czy to kontener (mo�esz u�y� AutomationId lub innego mechanizmu identyfikacji)
                        if (container.AutomationId != "Sklep")
                        {
                            // Zapisz kontener do bazy danych, przekazuj�c ShopId
                            SaveContainerToDatabase(container, container.Width / skala, container.Height / skala, shopId);
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("B��d", "Nie znaleziono sklepu o AutomationId = 'Sklep'.", "OK");
            }
        }
        private async Task<int> SaveShopToDatabase(BoxView shopBox)
        {
            var newShop = new Shop
            {
                Width = shopBox.Width / skala,
                Length = shopBox.Height / skala
            };

            // Pobierz dane od u�ytkownika dla poszczeg�lnych p�l
            newShop.Name = await DisplayPromptAsync("Nowy sklep", "Podaj nazw� sklepu:");

            // Sprawd�, czy u�ytkownik nacisn�� Anuluj lub nie wpisa� nic
            if (newShop.Name == null)
            {
                // Anulowano lub nie wpisano warto�ci, zako�cz operacj�
                return -1; // Warto�� -1 jako oznaczenie niepowodzenia
            }

            newShop.City = await DisplayPromptAsync("Nowy sklep", "Podaj miasto:");
            if (newShop.City == null) return -1;

            newShop.Street = await DisplayPromptAsync("Nowy sklep", "Podaj ulic�:");
            if (newShop.Street == null) return -1;

            newShop.PostalCode = await DisplayPromptAsync("Nowy sklep", "Podaj kod pocztowy:");
            if (newShop.PostalCode == null) return -1;

            // Dodaj sklep do bazy danych
            dbContext.Shops.Add(newShop);

            // Zapisz zmiany w bazie danych, aby uzyska� dost�p do nadanego identyfikatora
            await dbContext.SaveChangesAsync();

            // Zwr�� nadane ShopId
            return newShop.ShopId;
        }

        private async void ShelfTapped(object sender, EventArgs e)
        {
            if (sender is BoxView selectedShelf)
            {
                if (!trybUsuwanie)
                {
                    string dimensionsInput = await DisplayPromptAsync("Zmiana rozmiaru", "Podaj nowe wymiary (szeroko�� x wysoko��):");

                    if (!string.IsNullOrEmpty(dimensionsInput))
                    {
                        string[] dimensions = dimensionsInput.Split('x');

                        if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double newWidth) && double.TryParse(dimensions[1], out double newHeight))
                        {
                            selectedShelf.WidthRequest = newWidth * skala;
                            selectedShelf.HeightRequest = newHeight * skala;

                            // Kod do zmiany typu kontenera zosta� tutaj pozostawiony
                            await ChangeContainerType(selectedShelf);
                        }
                        else
                        {
                            await DisplayAlert("B��d", "Wprowad� poprawne warto�ci liczbowe w formacie 'szeroko�� x wysoko��'.", "OK");
                        }
                    }
                }
                else if (trybUsuwanie)
                {
                    if (selectedShelf.AutomationId != "Sklep")
                    {
                        Layout.Children.Remove(selectedShelf);
                    }
                }
            }
        }

        private async Task ChangeContainerType(BoxView selectedShelf)
        {
            // Kod do zmiany typu kontenera zosta� tutaj pozostawiony
            List<string> availableTypes = new List<string> { "P�ka", "Lod�wka", "Zamra�arka", "Stojak" };
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
            selectedShelf.AutomationId = newType; // W tym przyk�adzie u�ywam AutomationId do przechowywania typu kontenera
        }

        private async void MapCreate()
        {
            string dimensionsInput = await DisplayPromptAsync("Tworzenie sklepu", "Podaj wymiary (szeroko�� x wysoko��):");

            if (!string.IsNullOrEmpty(dimensionsInput))
            {
                string[] dimensions = dimensionsInput.Split('x');

                if (dimensions.Length == 2 && double.TryParse(dimensions[0], out double szerokosc) && double.TryParse(dimensions[1], out double wysokosc))
                {
                    Color kolorSzary = Color.FromHex("CCCCCC");
                    // Tworzenie prostok�ta
                    var Sklep = new BoxView
                    {
                        Color = kolorSzary, // Kolor prostok�ta
                        WidthRequest = szerokosc * skala, // Ustawienie szeroko�ci na podan� warto��
                        HeightRequest = wysokosc * skala // Ustawienie wysoko�ci na podan� warto��
                    };
                    Sklep.AutomationId = "Sklep";
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

        private void RemoveContainerFromMemory(BoxView container)
        {
            // Usu� kontener z pami�ci (je�li to konieczne)
            // Ta funkcja nie wykonuje operacji bazodanowych
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
                    Color kolorFioletowy = Color.FromHex("#9966FF");
                    // Tworzenie prostok�ta
                    var prostokat = new BoxView
                    {
                        Color = kolorFioletowy, // Kolor prostok�ta
                        WidthRequest = szerokosc * skala, // Ustawienie szeroko�ci na podan� warto��
                        HeightRequest = wysokosc * skala // Ustawienie wysoko�ci na podan� warto��
                    };

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

        private void SaveMapButton_Clicked(object sender, EventArgs e)
        {
            SaveMapToDatabase();
            // Dodaj ewentualne dodatkowe dzia�ania po zapisaniu mapy
            // ...
        }
    }
}
