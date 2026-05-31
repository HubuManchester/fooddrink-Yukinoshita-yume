using foodApp.Pages;
using foodApp.Services;

namespace foodApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Task.Run(async () => await UserService.LoadAsync());
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new LoginPage()));
    }
}
