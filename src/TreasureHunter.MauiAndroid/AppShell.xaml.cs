using TreasureHunter.MauiAndroid.Pages;

namespace TreasureHunter.MauiAndroid;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        // MainPage is now a ShellContent, no need to register as a route
    }
}
