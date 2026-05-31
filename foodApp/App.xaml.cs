using foodApp.Pages;
using foodApp.Services;

namespace foodApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        UserAppTheme = Preferences.Default.Get("app_theme", 0) switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        AccessibilityService.LargeTextEnabled =
            Preferences.Default.Get("large_text_enabled", false);

        Task.Run(async () =>
        {
            await UserService.LoadAsync();
            await FoodService.LoadAsync();
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new LoginPage()));
    }
}
