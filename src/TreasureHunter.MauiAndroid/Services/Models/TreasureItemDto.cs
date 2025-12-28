namespace TreasureHunter.MauiAndroid.Services.Models;

public class TreasureItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ItemTypeId { get; set; }
    public string ItemTypeName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double DiscoveryRadiusMeters { get; set; }
    public int PointValue { get; set; }
    public bool IsCollected { get; set; }
    public string PlacedByUserId { get; set; } = string.Empty;
    public string PlacedByUserName { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
    public string? ModelUrl { get; set; }
    public string? IconUrl { get; set; }
    
    // Calculated fields
    public double? DistanceMeters { get; set; }
    public double? BearingDegrees { get; set; }
    public bool IsInRange => DistanceMeters.HasValue && DistanceMeters.Value <= DiscoveryRadiusMeters;
}
