using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.Application.Features.TreasureItems.Queries;

public class GetNearbyTreasuresQueryHandler : IRequestHandler<GetNearbyTreasuresQuery, IEnumerable<TreasureItemDto>>
{
    private readonly IRepository<TreasureItem> _treasureRepository;
    private readonly ILocationService _locationService;

    public GetNearbyTreasuresQueryHandler(
        IRepository<TreasureItem> treasureRepository,
        ILocationService locationService)
    {
        _treasureRepository = treasureRepository;
        _locationService = locationService;
    }

    public async Task<IEnumerable<TreasureItemDto>> Handle(GetNearbyTreasuresQuery request, CancellationToken cancellationToken)
    {
        // Get all uncollected treasure items
        var allItems = await _treasureRepository.FindAsync(t => !t.IsCollected, cancellationToken);

        // Filter by distance and calculate bearing
        var nearbyItems = allItems
            .Select(item =>
            {
                var distance = _locationService.CalculateDistance(
                    request.UserLatitude,
                    request.UserLongitude,
                    item.Latitude,
                    item.Longitude);

                var bearing = _locationService.CalculateBearing(
                    request.UserLatitude,
                    request.UserLongitude,
                    item.Latitude,
                    item.Longitude);

                return new TreasureItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ItemTypeId = item.ItemTypeId,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    Altitude = item.Altitude,
                    DiscoveryRadiusMeters = item.DiscoveryRadiusMeters,
                    PointValue = item.PointValue,
                    IsCollected = item.IsCollected,
                    PlacedByUserId = item.PlacedByUserId,
                    PlacedAt = item.PlacedAt,
                    ModelUrl = item.ModelUrl,
                    IconUrl = item.IconUrl,
                    DistanceMeters = distance,
                    BearingDegrees = bearing
                };
            })
            .Where(dto => dto.DistanceMeters <= request.SearchRadiusMeters)
            .OrderBy(dto => dto.DistanceMeters)
            .ToList();

        return nearbyItems;
    }
}
