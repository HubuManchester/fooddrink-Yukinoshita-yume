using foodApp.Services;

namespace foodApp.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        ThemePicker.SelectedIndex = 0;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        ApplyLargeTextState();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        Announce("Theme has been updated.");
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        ApplyLargeTextState();
        Announce(e.Value
            ? "Large text mode is on. Text is now larger."
            : "Large text mode is off. Text has returned to normal size.");
    }

    private void ApplyLargeTextState()
    {
        AccessibilityService.ApplyFontScale(this);

        LargeTextPreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? "Large text preview: enlarged"
            : "Large text preview";
        LargeTextPreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? "Text is now noticeably larger across all pages."
            : "Turn on the switch to enlarge this preview and all page text.";
    }

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        UserService.Logout();
        Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage());
    }

    private void Announce(string message)
    {
        SettingsStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
