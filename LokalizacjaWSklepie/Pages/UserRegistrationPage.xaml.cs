using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;

namespace LokalizacjaWSklepie.Pages;

public partial class UserRegistrationPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    public UserRegistrationPage()
	{
		InitializeComponent();
	}
    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        if (passwordEntry.Text == passwordVerifyEntry.Text)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var newProduct = new User
                    {
                        Username = usernameEntry.Text,
                        Email = emailEntry.Text,
                        Password = passwordEntry.Text,
                        Role = "Client"
                    };

                    string json = JsonConvert.SerializeObject(newProduct);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{apiBaseUrl}/api/Users/AddUser", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Rejestracja przebieg³a poprawnie.", "OK");
                        var MainPage = new MainPage();
                        await Navigation.PushAsync(MainPage);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Nie uda³o siê zarejestrowaæ u¿ytkownika.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

    }
    private async void Back_Clicked(object sender, EventArgs e)
    {
        var MainPage = new MainPage();
        await Navigation.PushAsync(MainPage);
    }
}