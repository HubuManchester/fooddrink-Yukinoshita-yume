using foodApp.Services;

namespace foodApp.Pages;

public partial class MainPage : ContentPage
{
    private const double ShakeThreshold = 1.3;
    private DateTime lastShakeTime = DateTime.MinValue;
    private static readonly TimeSpan ShakeCooldown = TimeSpan.FromSeconds(2.5);

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

        StartShakeDetection();
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShakeDetection();
    }

    private void StartShakeDetection()
    {
        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        }
        catch
        {
            // Accelerometer not available on this device — shake won't work,
            // but the Random button is still usable.
        }
    }

    private void StopShakeDetection()
    {
        try
        {
            if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
                Accelerometer.Default.Stop();
            }
        }
        catch
        {
        }
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var reading = e.Reading;
        var magnitude = Math.Sqrt(
            reading.Acceleration.X * reading.Acceleration.X +
            reading.Acceleration.Y * reading.Acceleration.Y +
            reading.Acceleration.Z * reading.Acceleration.Z);

        var delta = Math.Abs(magnitude - 1.0);
        if (delta > ShakeThreshold && DateTime.Now - lastShakeTime > ShakeCooldown)
        {
            lastShakeTime = DateTime.Now;
            MainThread.BeginInvokeOnMainThread(async () => await ShowRandomFoodAsync());
        }
    }

    private async Task ShowRandomFoodAsync()
    {
        try
        {
            var food = FoodService.GetRandom();
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);

            var goToDetails = await DisplayAlert(
                $"Shake! — {food.Name}",
                $"Region: {food.RegionLabel}\nUploaded by: {food.UploadedBy}\n\n{food.Description}",
                "View Details",
                "Close");

            if (goToDetails)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(food.Id)}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load random food: {ex.Message}", "OK");
        }
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

    private async void OnRandomClicked(object? sender, EventArgs e)
    {
        await ShowRandomFoodAsync();
    }
}
