# Food Encyclopedia (foodApp)

A cross-platform mobile application built with .NET MAUI for browsing, searching, and sharing food from around the world.

## Features

- **Food Encyclopedia**: Browse food entries with name, region, description, and uploader info
- **Search**: Filter foods by name, region, description, or tags
- **User System**: Register and login with username and password (10 test users included, all with password "123")
- **Upload Food**: Add new food entries with name, region, description, and camera/gallery photo
- **Map Location Picker**: Select region via native map (Google Maps / Apple Maps), or type manually
- **Shake-to-Random**: Shake the device to get a random food recommendation
- **Pull-to-Refresh**: Pull down to re-randomize all food cards
- **Scroll-to-Load-More**: Scroll to the bottom to load the next page (16 items per page), previous items are kept
- **Tablet Adaptive Grid**: Automatically switches between 1/2/3/4 columns based on screen width
- **Long-Press Browser Search**: Long-press a food card → vibration → dialog → opens browser to search the food name
- **Voice Playback**: Tap the speaker button on the detail page to hear the description read aloud (TTS)
- **Theme Support**: Light, dark, and system-following theme modes
- **Large Text Mode**: Accessibility font scaling (1.22x) across all pages

## Tech Stack

- **Framework**: .NET MAUI (net9.0)
- **Data Source**: mockapi.io REST API with local fallback data when network is unavailable
- **Test Data**: 50 food entries + 10 test users embedded as JSON resource files
- **Local Storage**: User and food data persisted via JSON files

## Project Structure

```
foodApp/
  Behaviors/
    LongPressBehavior.cs   # Cross-platform long-press behavior
  Models/
    FoodItem.cs            # Food data model
    User.cs                # User model
  Services/
    MockApiConfig.cs       # mockapi.io configuration
    FoodService.cs         # Food data service (API + JSON resource + local fallback)
    UserService.cs         # User authentication service (JSON resource + local persistence)
    AccessibilityService.cs # Font scaling for accessibility
    SpeechService.cs       # Text-to-speech service
    MapConfig.cs           # Google Maps API Key config
  Pages/
    LoginPage.xaml         # Login page
    RegisterPage.xaml      # Registration page
    MainPage.xaml          # Home page (food card grid + pagination)
    AddFoodPage.xaml       # Add food entry
    FoodDetailPage.xaml    # Food detail page (with voice playback button)
    MapPage.xaml           # Map location picker
    SettingsPage.xaml      # Settings page (theme + large text)
  Platforms/Android/       # Android platform config
  Resources/
    Raw/
      test_food_data.json  # 50 test food entries
      test_users.json      # 10 test user accounts
    Images/                # Food SVG illustrations
    Styles/                # Global styles
    Fonts/                 # Font files
```

## Build & Run

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 with .NET MAUI workload

### Windows Build

```powershell
dotnet build foodApp\foodApp.csproj -f net9.0-windows10.0.19041.0
```

### Android Build

```powershell
dotnet build foodApp\foodApp.csproj -f net9.0-android
```

## MockAPI Configuration

See `mockapi配置说明.md` in the project root.

## Demo Script

1. Launch the app — login/register page appears (use "chef_li" / "123" to log in directly)
2. Browse the food card grid — cards are randomly ordered
3. Pull down to refresh — cards re-randomize
4. Scroll down — more cards load automatically
5. **Long-press** a food card → vibration → confirm dialog → browser opens with search results
6. Search for a specific food
7. Tap Details → view full info → tap 🔊 Play to hear the description read aloud
8. Add a new food entry (take photo / pick from gallery + select region on map)
9. Shake the device → random food recommendation pops up
10. Switch between light and dark themes
11. Enable large text mode
