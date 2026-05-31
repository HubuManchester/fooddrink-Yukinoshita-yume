namespace foodApp.Services;

public static class MapConfig
{
    // Replace with your Google Maps API key for Android.
    // Get one at: https://console.cloud.google.com/google/maps-apis
    // Leave empty to let the map attempt to work without a key
    // (limited functionality, may show blank tiles on Android).
    public const string GoogleMapsApiKey = "";

    public static bool IsKeyConfigured => !string.IsNullOrWhiteSpace(GoogleMapsApiKey);
}
