using foodApp.Pages;

namespace foodApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AddFoodPage), typeof(AddFoodPage));
        Routing.RegisterRoute(nameof(FoodDetailPage), typeof(FoodDetailPage));
        Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
    }
}
