using foodApp.Services;
using Microsoft.Maui.Controls.Maps;

namespace foodApp.Pages;

public partial class MapPage : ContentPage
{
    public static string? SelectedRegion { get; set; }

    private double? selectedLat;
    private double? selectedLng;

    public MapPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        SelectedRegion = null;
        selectedLat = null;
        selectedLng = null;
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        selectedLat = e.Location.Latitude;
        selectedLng = e.Location.Longitude;
        SelectedCoordLabel.Text = $"Selected: {selectedLat:F5}, {selectedLng:F5}";
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (selectedLat is null || selectedLng is null)
        {
            StatusLabel.Text = "Please tap on the map to select a location first.";
            StatusLabel.TextColor = Colors.Red;
            return;
        }

        try
        {
            var lat = selectedLat.Value;
            var lng = selectedLng.Value;

            var regionName = await ReverseGeocodeAsync(lat, lng);
            if (regionName is null)
            {
                regionName = $"{lat:F5}, {lng:F5}";
            }

            SelectedRegion = regionName;
            await Navigation.PopAsync();
        }
        catch
        {
            StatusLabel.Text = "Could not determine the location. Please check your network or type the region name manually.";
            StatusLabel.TextColor = Colors.Red;
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
}
