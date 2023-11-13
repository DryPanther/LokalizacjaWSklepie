using LokalizacjaWSklepie.Models;

namespace LokalizacjaWSklepie.Pages;

public partial class MapCreator : ContentPage
{
    int skala = 100;
    bool trybUsuwanie = false;
    LokalizacjaWsklepieContext dbContext = new LokalizacjaWsklepieContext();
    public MapCreator()
    {
        InitializeComponent();
        MapCreate();

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
}