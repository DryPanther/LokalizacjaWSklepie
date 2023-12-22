using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LokalizacjaWSklepie.Pages;

public partial class UserListPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private ObservableCollection<User> users;
    private List<User> allUsers;
    public UserListPage()
    {
        InitializeComponent();
        users = new ObservableCollection<User>();
        UsersListView.ItemsSource = users;
        LoadUsers();
    }

    private async void LoadUsers()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiBaseUrl}/api/Users/GetAllUsers");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var shopList = JsonConvert.DeserializeObject<List<User>>(responseData);

                    users.Clear();
                    foreach (var shop in shopList)
                    {
                        users.Add(shop);
                    }

                    UsersListView.ItemsSource = users;
                }
                else
                {
                    await DisplayAlert("B³¹d", "Nie znaleziono u¿ytkownika", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", ex.Message, "OK");
        }
    }

    private async void UsersListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        try
        {
            if (e.Item is User selectedUser)
            {

                await Navigation.PushAsync(new UserEditPage(selectedUser));

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in UsersListView_ItemTapped: {ex.Message}");
        }
    }

    private async void Create_Clicked(object sender, EventArgs e)
    {
        var UserCreatePage = new UserCreatePage();
        await Navigation.PushAsync(UserCreatePage);
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        var AdminMenuPage = new AdminMenuPage();
        await Navigation.PushAsync(AdminMenuPage);
    }

    private async void OnUserSearchButtonPressed(object sender, EventArgs e)
    {
        string searchText = userSearchBar.Text;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            await SearchUsers(searchText);
        }
        else
        {
            LoadUsers();
        }
    }

    private async void OnUserSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            await SearchUsers(searchText);
        }
        else
        {
            LoadUsers();
        }
    }

    private async Task SearchUsers(string searchText)
    {
        var filteredUsers = allUsers
                .Where(User => User.Username.ToLower().Contains(searchText.ToLower()) || User.Email.ToLower().Contains(searchText.ToLower()))
                .ToList();
    }


}