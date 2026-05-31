using foodApp.Services;

namespace foodApp.Pages;

public partial class MapPage : ContentPage
{
    public static string? SelectedRegion { get; set; }

    private bool mapConfirmed;

    public MapPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        SelectedRegion = null;
        mapConfirmed = false;
        CheckMapLoaded();
    }

    private async void CheckMapLoaded()
    {
        await Task.Delay(4000);
        if (mapConfirmed) return;

        try
        {
            var failed = await MapWebView.EvaluateJavaScriptAsync("mapLoadFailed()");
            if (failed?.Trim() == "true")
            {
                await DisplayAlert(
                    "Map unavailable",
                    "The map could not be loaded. Please check your network connection, or go back and enter the region name manually.",
                    "OK");
            }
        }
        catch
        {
            // If EvaluateJavaScriptAsync itself fails, the WebView may not be ready
        }
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        try
        {
            var resultJson = await MapWebView.EvaluateJavaScriptAsync("getSelectedLocation()");
            if (string.IsNullOrWhiteSpace(resultJson))
            {
                StatusLabel.Text = "Please tap on the map to select a location first.";
                return;
            }

            var latStr = ExtractJsonValue(resultJson, "lat");
            var lngStr = ExtractJsonValue(resultJson, "lng");
            var hasSel = ExtractJsonValue(resultJson, "hasSelection");

            if (hasSel != "true" || !double.TryParse(latStr, out var lat) || !double.TryParse(lngStr, out var lng))
            {
                StatusLabel.Text = "Please tap on the map to select a location first.";
                return;
            }

            mapConfirmed = true;

            var regionName = await ReverseGeocodeAsync(lat, lng);
            if (regionName is null)
            {
                regionName = $"{lat:F5}, {lng:F5}";
                StatusLabel.Text = $"Cannot determine place name. Using coordinates: {regionName}";
            }

            SelectedRegion = regionName;
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        SelectedRegion = null;
        await Navigation.PopAsync();
    }

    private static async Task<string?> ReverseGeocodeAsync(double lat, double lng)
    {
        try
        {
            var location = new Location(lat, lng);
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            if (placemark is null) return null;

            var parts = new[]
            {
                placemark.CountryName,
                placemark.AdminArea,
                placemark.Locality,
                placemark.SubLocality
            }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToArray();

            return parts.Length > 0 ? string.Join(", ", parts) : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractJsonValue(string json, string key)
    {
        var searchKey = $"\"{key}\":";
        var idx = json.IndexOf(searchKey, StringComparison.Ordinal);
        if (idx < 0) return null;
        idx += searchKey.Length;
        while (idx < json.Length && json[idx] == ' ') idx++;
        if (idx >= json.Length) return null;

        if (json[idx] == '"')
        {
            idx++;
            var end = json.IndexOf('"', idx);
            return end >= 0 ? json.Substring(idx, end - idx) : null;
        }

        var endIdx = idx;
        while (endIdx < json.Length && (char.IsDigit(json[endIdx]) || json[endIdx] == '.' || json[endIdx] == '-'))
            endIdx++;
        return json.Substring(idx, endIdx - idx);
    }
}
