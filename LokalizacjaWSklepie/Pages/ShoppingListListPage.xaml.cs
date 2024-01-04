using LokalizacjaWSklepie.Models;
using LokalizacjaWSklepie.Properties;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LokalizacjaWSklepie.Pages;

public partial class ShoppingListListPage : ContentPage
{
    private readonly string apiBaseUrl = ApiConfiguration.ApiBaseUrl;
    private ObservableCollection<ShoppingList> shoppingLists;
    public ShoppingListListPage()
	{
		InitializeComponent();
        shoppingLists = new ObservableCollection<ShoppingList>();
        ShoppingListsListView.ItemsSource = shoppingLists;
        LoadShoppingLists();
    }
    private async void LoadShoppingLists()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiBaseUrl}/api/ShoppingLists/GetShoppingListsByUserId/{Memory.Instance.user.UserId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var shoppingListList = JsonConvert.DeserializeObject<List<ShoppingList>>(responseData);

                    shoppingLists.Clear();
                    foreach (var shoppingList in shoppingListList)
                    {
                        shoppingLists.Add(shoppingList);
                    }

                    ShoppingListsListView.ItemsSource = shoppingLists;
                }
                else
                {
                    await DisplayAlert("B³¹d", "Nie znaleziono Listy zakupów", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", ex.Message, "OK");
        }
    }

    private async void ShoppingListsListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        try
        {
            if (e.Item is ShoppingList selectedShoppingList)
            {

                Console.WriteLine($"Item tapped: {selectedShoppingList.ListName}, ShoppingListId: {selectedShoppingList.ShoppingListId}");

                await Navigation.PushAsync(new ShoppingListEditPage(selectedShoppingList.ShoppingListId));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ShoppingListsListView_ItemTapped: {ex.Message}");
        }
    }

    private async void Create_Clicked(object sender, EventArgs e)
    {
        var ShoppingListCreatePage = new ShoppingListCreatePage();
        await Navigation.PushAsync(ShoppingListCreatePage);
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        if (Memory.Instance.user.Role == "Admin")
        {
            var AdminMenuPage = new AdminMenuPage();
            await Navigation.PushAsync(AdminMenuPage);
        }
        else if (Memory.Instance.user.Role == "Client")
        {
            var ClientMenuPage = new ClientMenuPage();
            await Navigation.PushAsync(ClientMenuPage);
        }

    }

    private async void OnShoppingListSearchButtonPressed(object sender, EventArgs e)
    {
        string searchText = shoppingListSearchBar.Text;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            await SearchShoppingLists(searchText);
        }
        else
        {
            LoadShoppingLists();
        }
    }

    private async void OnShoppingListSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            await SearchShoppingLists(searchText);
        }
        else
        {
            LoadShoppingLists();
        }
    }

    private async Task SearchShoppingLists(string searchText)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{apiBaseUrl}/api/ShoppingLists/GetShoppingListByListName/{searchText}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var shoppingListList = JsonConvert.DeserializeObject<List<ShoppingList>>(responseData);

                    shoppingLists.Clear();
                    foreach (var shoppingList in shoppingListList)
                    {
                        shoppingLists.Add(shoppingList);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to retrieve shops", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}