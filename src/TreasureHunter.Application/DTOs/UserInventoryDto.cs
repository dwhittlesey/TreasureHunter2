using System;

namespace TreasureHunter.Application.DTOs;

public class UserInventoryDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid TreasureItemId { get; set; }
    public string TreasureItemName { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; }
    public int PointsEarned { get; set; }
}
