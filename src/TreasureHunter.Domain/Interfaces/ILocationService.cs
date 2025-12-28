namespace TreasureHunter.Domain.Interfaces;

public interface ILocationService
{
    /// <summary>
    /// Calculates the distance between two GPS coordinates using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point</param>
    /// <param name="lon1">Longitude of the first point</param>
    /// <param name="lat2">Latitude of the second point</param>
    /// <param name="lon2">Longitude of the second point</param>
    /// <returns>Distance in meters</returns>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Determines if a user is within a specified radius of a target location.
    /// </summary>
    /// <param name="userLat">User's latitude</param>
    /// <param name="userLon">User's longitude</param>
    /// <param name="targetLat">Target latitude</param>
    /// <param name="targetLon">Target longitude</param>
    /// <param name="radiusMeters">Radius in meters</param>
    /// <returns>True if within radius, false otherwise</returns>
    bool IsWithinRadius(double userLat, double userLon, double targetLat, double targetLon, double radiusMeters);

    /// <summary>
    /// Calculates the bearing (direction) from one point to another.
    /// </summary>
    /// <param name="lat1">Starting latitude</param>
    /// <param name="lon1">Starting longitude</param>
    /// <param name="lat2">Target latitude</param>
    /// <param name="lon2">Target longitude</param>
    /// <returns>Bearing in degrees (0-360)</returns>
    double CalculateBearing(double lat1, double lon1, double lat2, double lon2);
}
