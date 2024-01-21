using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Pages;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;

namespace LokalizacjaWSklepie;

public partial class MainPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    string email;
    public MainPage()
    {
        
        InitializeComponent();
    }

        private async void Login_Clicked(object sender, EventArgs e)
    {
        email = Email.Text;
        var user = await GetUser();
        if (user == null)
        {
            await DisplayAlert("Błąd", "Błąd podczas łączenia z bazą lub nie ma takiego użytkownika", "OK");
        }
        else
        {
            if (BCrypt.Net.BCrypt.EnhancedVerify(Password.Text, user.Password))
            {
                if (user.Role == "Admin")
                {
                    Memory.Instance.user = user;
                    Password.Text = null;
                    var AdminMenuPage = new AdminMenuPage();
                    await Navigation.PushAsync(AdminMenuPage);
                }
                else if (user.Role == "Client")
                {
                    Memory.Instance.user = user;
                    Password.Text = null;
                    var ClientMenuPage = new ClientMenuPage();
                    await Navigation.PushAsync(ClientMenuPage);
                }
                else
                {
                    await DisplayAlert("Błąd", "Błąd podczas uzyskiwania roli", "OK");
                }
            }
            else
            {
                await DisplayAlert("Błąd", "Kombinacja email i hasła niepoprawna", "OK");
            }
        }

    }

    private async void Register_Clicked(object sender, EventArgs e)
    {
        var UserRegistrationPage = new UserRegistrationPage();
        await Navigation.PushAsync(UserRegistrationPage);
    }
    private async Task<User> GetUser()
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync($"{apiBaseUrl}/api/Users/GetUserByEmail/{email}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<User>(responseData);
            }
            else
            {
                return null;
            }
        }
    }
}

