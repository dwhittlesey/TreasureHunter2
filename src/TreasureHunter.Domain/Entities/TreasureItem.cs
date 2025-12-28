using System;

namespace TreasureHunter.Domain.Entities;

public class TreasureItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ItemTypeId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double DiscoveryRadiusMeters { get; set; } = 5.0;
    public int PointValue { get; set; }
    public bool IsCollected { get; set; } = false;
    public string PlacedByUserId { get; set; } = string.Empty;
    public string? CollectedByUserId { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime? CollectedAt { get; set; }
    public string? ModelUrl { get; set; }
    public string? IconUrl { get; set; }

    // Navigation properties
    public virtual ItemType ItemType { get; set; } = null!;
    public virtual ApplicationUser PlacedByUser { get; set; } = null!;
    public virtual ApplicationUser? CollectedByUser { get; set; }
}
