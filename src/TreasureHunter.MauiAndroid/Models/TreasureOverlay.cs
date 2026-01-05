namespace TreasureHunter.MauiAndroid.Models;

public class TreasureOverlay
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Distance { get; set; }
    public double Bearing { get; set; }
    public string IconUrl { get; set; } = string.Empty;
    public int PointValue { get; set; }
    public string ProximityLevel { get; set; } = string.Empty;
    
    // Screen position for AR overlay
    public float ScreenX { get; set; }
    public float ScreenY { get; set; }
    public bool IsVisible { get; set; }
}