using System;
using TreasureHunter.Domain.Constants;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.Infrastructure.Services;

public class LocationService : ILocationService
{
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula implementation
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Distance in meters
        return GameConstants.EarthRadiusKilometers * c * 1000;
    }

    public bool IsWithinRadius(double userLat, double userLon, double targetLat, double targetLon, double radiusMeters)
    {
        var distance = CalculateDistance(userLat, userLon, targetLat, targetLon);
        return distance <= radiusMeters;
    }

    public double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        // Calculate bearing (direction) from point 1 to point 2
        var dLon = ToRadians(lon2 - lon1);
        var lat1Rad = ToRadians(lat1);
        var lat2Rad = ToRadians(lat2);

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearingRad = Math.Atan2(y, x);
        var bearingDeg = ToDegrees(bearingRad);

        // Normalize to 0-360
        return (bearingDeg + 360) % 360;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    private static double ToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}
