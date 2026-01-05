using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

namespace TreasureHunter.MauiAndroid;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | 
                          ConfigChanges.Orientation | 
                          ConfigChanges.UiMode | 
                          ConfigChanges.ScreenLayout | 
                          ConfigChanges.SmallestScreenSize | 
                          ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnDestroy()
    {
        // Clear focus from all views before destruction to prevent race condition
        try
        {
            var currentFocus = CurrentFocus;
            if (currentFocus != null)
            {
                currentFocus.ClearFocus();
            }
            
            // Small delay to allow focus events to complete
            System.Threading.Thread.Sleep(50);
        }
        catch
        {
            // Ignore any exceptions during cleanup
        }
        
        base.OnDestroy();
    }
}
