using System.Collections.Generic;

namespace TreasureHunter.Domain.Entities;

public class ItemType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int BasePointValue { get; set; }
    public string? DefaultModelUrl { get; set; }
    public string? DefaultIconUrl { get; set; }

    // Navigation properties
    public virtual ICollection<TreasureItem> TreasureItems { get; set; } = new List<TreasureItem>();
}
