using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.API.Hubs;

[Authorize]
public class TreasureHub : Hub
{
    private readonly IRepository<TreasureItem> _treasureRepository;
    private readonly ILocationService _locationService;

    public TreasureHub(IRepository<TreasureItem> treasureRepository, ILocationService locationService)
    {
        _treasureRepository = treasureRepository;
        _locationService = locationService;
    }

    public async Task UpdateLocation(double latitude, double longitude)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // Get all uncollected treasures
        var allTreasures = await _treasureRepository.FindAsync(t => !t.IsCollected);

        // Find nearby treasures and calculate proximity
        var nearbyTreasures = allTreasures
            .Select(treasure =>
            {
                var distance = _locationService.CalculateDistance(
                    latitude, longitude,
                    treasure.Latitude, treasure.Longitude);

                var bearing = _locationService.CalculateBearing(
                    latitude, longitude,
                    treasure.Latitude, treasure.Longitude);

                return new
                {
                    Treasure = treasure,
                    Distance = distance,
                    Bearing = bearing,
                    WithinRadius = distance <= treasure.DiscoveryRadiusMeters
                };
            })
            .Where(x => x.Distance <= 100) // Only show items within 100 meters
            .OrderBy(x => x.Distance)
            .ToList();

        // Send proximity feedback
        var proximityData = nearbyTreasures.Select(x => new
        {
            Id = x.Treasure.Id,
            Name = x.Treasure.Name,
            Distance = x.Distance,
            Bearing = x.Bearing,
            WithinDiscoveryRadius = x.WithinRadius,
            DiscoveryRadiusMeters = x.Treasure.DiscoveryRadiusMeters,
            ProximityLevel = GetProximityLevel(x.Distance, x.Treasure.DiscoveryRadiusMeters),
            IconUrl = x.Treasure.IconUrl,
            ModelUrl = x.Treasure.ModelUrl
        }).ToList();

        await Clients.Caller.SendAsync("ProximityUpdate", proximityData);

        // Notify if any treasure is within discovery radius
        var discoverable = nearbyTreasures.Where(x => x.WithinRadius).ToList();
        if (discoverable.Any())
        {
            await Clients.Caller.SendAsync("TreasureDiscoverable", discoverable.Select(x => new
            {
                Id = x.Treasure.Id,
                Name = x.Treasure.Name,
                Distance = x.Distance,
                PointValue = x.Treasure.PointValue
            }));
        }
    }

    private static string GetProximityLevel(double distance, double discoveryRadius)
    {
        var ratio = distance / discoveryRadius;

        if (ratio <= 0.25) return "VERY_HOT";
        if (ratio <= 0.5) return "HOT";
        if (ratio <= 0.75) return "WARM";
        if (ratio <= 1.0) return "COOL";
        if (ratio <= 2.0) return "COLD";
        return "VERY_COLD";
    }
}
