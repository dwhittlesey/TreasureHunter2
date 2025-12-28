using TreasureHunter.MauiAndroid.Pages;

namespace TreasureHunter.MauiAndroid;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("MainPage", typeof(MainPage));
    }
}
