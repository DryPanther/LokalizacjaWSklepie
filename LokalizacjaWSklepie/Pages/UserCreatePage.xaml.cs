using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;

namespace LokalizacjaWSklepie.Pages;

public partial class UserCreatePage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    public UserCreatePage()
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
                        Role = userRolePicker.SelectedItem.ToString()
                    };

                    string json = JsonConvert.SerializeObject(newProduct);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{apiBaseUrl}/api/Users/AddUser", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Product zosta³ dodany.", "OK");
                        var ProductsPage = new ProductsPage();
                        await Navigation.PushAsync(ProductsPage);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Nie uda³o siê dodaæ produktu.", "OK");
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
        var UserListPage = new UserListPage();
        await Navigation.PushAsync(UserListPage);
    }
}