namespace TreasureHunter.Domain.Constants;

public static class GameConstants
{
    // Discovery radius settings
    public const double DefaultDiscoveryRadiusMeters = 5.0;
    public const double MaxDiscoveryRadiusMeters = 50.0;
    public const double MinDiscoveryRadiusMeters = 1.0;

    // Point system
    public const int DefaultPointsPerItem = 100;
    public const int RareItemMultiplier = 3;
    public const int EpicItemMultiplier = 5;

    // Location updates
    public const int LocationUpdateIntervalSeconds = 5;

    // Earth calculations
    public const double EarthRadiusKilometers = 6371.0;

    // AR settings
    public const string DefaultARScale = "0.5 0.5 0.5";
    public const double ARRenderDistanceMeters = 100.0;
}
