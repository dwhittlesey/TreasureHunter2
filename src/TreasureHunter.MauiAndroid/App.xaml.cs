using Microsoft.Maui.Controls;

namespace TreasureHunter.MauiAndroid;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
