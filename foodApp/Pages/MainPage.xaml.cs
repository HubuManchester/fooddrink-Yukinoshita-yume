using foodApp.Services;

namespace foodApp.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        if (!UserService.IsLoggedIn)
        {
            Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage());
            return;
        }

        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async Task LoadFoodItemsAsync(string? query = null)
    {
        FoodCollection.ItemsSource = await FoodService.SearchAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddFoodPage));
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food list refreshed. Current source: {source}.");
    }
}
