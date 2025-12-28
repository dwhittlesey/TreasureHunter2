using System;
using System.Collections.Generic;

namespace TreasureHunter.Domain.Entities;

public class ApplicationUser
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public int TotalPoints { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }

    // Navigation properties
    public virtual ICollection<TreasureItem> PlacedItems { get; set; } = new List<TreasureItem>();
    public virtual ICollection<TreasureItem> CollectedItems { get; set; } = new List<TreasureItem>();
    public virtual ICollection<UserInventory> Inventory { get; set; } = new List<UserInventory>();
}
