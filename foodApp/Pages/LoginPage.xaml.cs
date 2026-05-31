using foodApp.Services;

namespace foodApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private void OnLoginClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var error = UserService.Login(UsernameEntry.Text ?? "", PasswordEntry.Text ?? "");
        if (error is not null)
        {
            ErrorLabel.Text = error;
            ErrorLabel.IsVisible = true;
            return;
        }

        Application.Current!.Windows[0].Page = new AppShell();
    }

    private async void OnGoToRegisterClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}
