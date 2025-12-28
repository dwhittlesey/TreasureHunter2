using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TreasureHunter.Domain.Entities;

namespace TreasureHunter.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TreasureItem> TreasureItems { get; set; } = null!;
    public DbSet<ItemType> ItemTypes { get; set; } = null!;
    public DbSet<UserInventory> UserInventories { get; set; } = null!;
    public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure TreasureItem
        builder.Entity<TreasureItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.DiscoveryRadiusMeters).HasDefaultValue(5.0);
            entity.Property(e => e.IsCollected).HasDefaultValue(false);

            entity.HasOne(e => e.ItemType)
                .WithMany(t => t.TreasureItems)
                .HasForeignKey(e => e.ItemTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PlacedByUser)
                .WithMany(u => u.PlacedItems)
                .HasForeignKey(e => e.PlacedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CollectedByUser)
                .WithMany(u => u.CollectedItems)
                .HasForeignKey(e => e.CollectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            entity.HasIndex(e => e.IsCollected);
        });

        // Configure ItemType
        builder.Entity<ItemType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure UserInventory
        builder.Entity<UserInventory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Inventory)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TreasureItem)
                .WithMany()
                .HasForeignKey(e => e.TreasureItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CollectedAt);
        });

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.TotalPoints).HasDefaultValue(0);
        });

        // Seed ItemTypes
        builder.Entity<ItemType>().HasData(
            new ItemType
            {
                Id = 1,
                Name = "Common Coin",
                Description = "A shiny golden coin",
                BasePointValue = 100,
                DefaultModelUrl = "/models/coin.gltf",
                DefaultIconUrl = "/icons/coin.png"
            },
            new ItemType
            {
                Id = 2,
                Name = "Rare Gem",
                Description = "A sparkling rare gemstone",
                BasePointValue = 300,
                DefaultModelUrl = "/models/gem.gltf",
                DefaultIconUrl = "/icons/gem.png"
            },
            new ItemType
            {
                Id = 3,
                Name = "Epic Treasure Chest",
                Description = "A legendary treasure chest filled with riches",
                BasePointValue = 500,
                DefaultModelUrl = "/models/chest.gltf",
                DefaultIconUrl = "/icons/chest.png"
            }
        );
    }
}
