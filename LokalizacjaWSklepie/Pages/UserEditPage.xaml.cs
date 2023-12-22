using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Text;

namespace LokalizacjaWSklepie.Pages;

public partial class UserEditPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private User user;
    public UserEditPage(User user)
    {
        InitializeComponent();
        this.user = user;
        BindingContext = user;
    }
    private async void SaveChangesButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(user);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{apiBaseUrl}/api/Users/UpdateUser/{user.UserId}", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "U�ytkownik zaktualizowany pomy�lnie", "OK");
                    var UserListPage = new UserListPage();
                    await Navigation.PushAsync(UserListPage);
                }
                else
                {
                    await DisplayAlert("Error", "Nieudana pr�ba edycji", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Deletion", "Czy jeste� pewny �e chcesz usun�� u�ytkownika?", "Tak", "Nie");

        if (confirm)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.DeleteAsync($"{apiBaseUrl}/api/Users/DeleteUser/{user.UserId}");

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "U�ytkownik usuni�ty pomy�lnie", "OK");
                        var UserListPage = new UserListPage();
                        await Navigation.PushAsync(UserListPage);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Nieudana pr�ba usuni�cia", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        var UserListPage = new UserListPage();
        await Navigation.PushAsync(UserListPage);
    }
}