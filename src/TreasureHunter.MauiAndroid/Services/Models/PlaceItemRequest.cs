namespace TreasureHunter.MauiAndroid.Services.Models;

public class PlaceItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ItemTypeId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double DiscoveryRadiusMeters { get; set; }
}

public class PlaceItemResponse
{
    public Guid ItemId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
