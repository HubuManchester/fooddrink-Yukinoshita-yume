using System.Collections.ObjectModel;
using foodApp.Models;
using foodApp.Services;

namespace foodApp.Pages;

public partial class MainPage : ContentPage
{
    private const double ShakeThreshold = 1.3;
    private DateTime lastShakeTime = DateTime.MinValue;
    private static readonly TimeSpan ShakeCooldown = TimeSpan.FromSeconds(2.5);

    private const int PageSize = 16;
    private int currentPage;
    private bool hasMoreItems = true;
    private bool isLoadingMore;
    private string? lastQuery;
    private readonly ObservableCollection<FoodItem> displayedItems = [];

    public MainPage()
    {
        InitializeComponent();
        FoodCollection.ItemsSource = displayedItems;
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

        UpdateGridLayoutSpan();
        StartShakeDetection();
        await ResetAndLoadAsync(SearchFoodBar.Text);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateGridLayoutSpan();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopShakeDetection();
    }

    private void UpdateGridLayoutSpan()
    {
        var width = Width;
        // Adaptive columns for tablet:
        //   width >= 1200 → 4 columns
        //   width >= 900  → 3 columns
        //   width >= 600  → 2 columns
        //   otherwise     → 1 column
        var span = width switch
        {
            >= 1200 => 4,
            >= 900 => 3,
            >= 600 => 2,
            _ => 1
        };

        if (FoodCollection.ItemsLayout is GridItemsLayout gridLayout)
        {
            gridLayout.Span = span;
        }
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
            if (food is null) return;
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

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string foodName
            || string.IsNullOrWhiteSpace(foodName)) return;

        var openBrowser = await DisplayAlert(
            "Search in Browser",
            $"Do you want to search \"{foodName}\" in your browser?",
            "Search",
            "Cancel");

        if (openBrowser)
        {
            try
            {
                var encodedName = Uri.EscapeDataString(foodName);
                var url = $"https://www.google.com/search?q={encodedName}+food";
                await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    $"Could not open browser: {ex.Message}", "OK");
            }
        }
    }

    private async Task ResetAndLoadAsync(string? query)
    {
        currentPage = 0;
        hasMoreItems = true;
        lastQuery = query;
        displayedItems.Clear();
        await LoadNextPageAsync();
    }

    private async Task LoadNextPageAsync()
    {
        if (isLoadingMore || !hasMoreItems) return;

        isLoadingMore = true;
        ShowLoadingIndicator(true);

        try
        {
            var (items, totalCount) = await FoodService.GetPagedAsync(
                currentPage, PageSize, lastQuery, randomOrder: true);

            foreach (var item in items)
            {
                displayedItems.Add(item);
            }

            currentPage++;
            hasMoreItems = displayedItems.Count < totalCount;

            AccessibilityService.ApplyFontScale(this);
            SemanticScreenReader.Announce(
                $"Showing {displayedItems.Count} of {totalCount} food entries.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load food entries: {ex.Message}", "OK");
        }
        finally
        {
            isLoadingMore = false;
            ShowLoadingIndicator(false);
        }
    }

    private void ShowLoadingIndicator(bool show)
    {
        LoadingMoreIndicator.IsVisible = show;
        LoadingMoreIndicator.IsRunning = show;
    }

    private async void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        await LoadNextPageAsync();
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
        await ResetAndLoadAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await ResetAndLoadAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await ResetAndLoadAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food list refreshed and re-randomized. Current source: {source}.");
    }

    private async void OnRandomClicked(object? sender, EventArgs e)
    {
        await ShowRandomFoodAsync();
    }
}
