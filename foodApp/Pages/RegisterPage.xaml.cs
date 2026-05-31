using foodApp.Services;

namespace foodApp.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var username = UsernameEntry.Text ?? "";
        var password = PasswordEntry.Text ?? "";
        var confirm = ConfirmPasswordEntry.Text ?? "";

        if (password != confirm)
        {
            ErrorLabel.Text = "Passwords do not match. Please check and try again.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var error = await UserService.RegisterAsync(username, password);
        if (error is not null)
        {
            ErrorLabel.Text = error;
            ErrorLabel.IsVisible = true;
            return;
        }

        Application.Current!.Windows[0].Page = new AppShell();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
