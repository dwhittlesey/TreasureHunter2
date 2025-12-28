using System;

namespace TreasureHunter.Domain.Entities;

public class UserInventory
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid TreasureItemId { get; set; }
    public DateTime CollectedAt { get; set; }
    public int PointsEarned { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual TreasureItem TreasureItem { get; set; } = null!;
}
