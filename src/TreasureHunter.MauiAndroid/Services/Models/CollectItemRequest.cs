namespace TreasureHunter.MauiAndroid.Services.Models;

public class CollectItemRequest
{
    public Guid ItemId { get; set; }
    public double UserLatitude { get; set; }
    public double UserLongitude { get; set; }
}

public class CollectItemResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public int TotalPoints { get; set; }
    public bool PlaySound { get; set; }
}
