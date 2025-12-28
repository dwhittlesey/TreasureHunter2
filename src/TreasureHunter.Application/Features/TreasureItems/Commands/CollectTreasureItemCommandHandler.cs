using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.Application.Features.TreasureItems.Commands;

public class CollectTreasureItemCommandHandler : IRequestHandler<CollectTreasureItemCommand, UserInventoryDto>
{
    private readonly IRepository<TreasureItem> _treasureRepository;
    private readonly IRepository<UserInventory> _inventoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly ILocationService _locationService;
    private readonly IUnitOfWork _unitOfWork;

    public CollectTreasureItemCommandHandler(
        IRepository<TreasureItem> treasureRepository,
        IRepository<UserInventory> inventoryRepository,
        IRepository<ApplicationUser> userRepository,
        ILocationService locationService,
        IUnitOfWork unitOfWork)
    {
        _treasureRepository = treasureRepository;
        _inventoryRepository = inventoryRepository;
        _userRepository = userRepository;
        _locationService = locationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserInventoryDto> Handle(CollectTreasureItemCommand request, CancellationToken cancellationToken)
    {
        // Get treasure item
        var treasureItem = await _treasureRepository.GetByIdAsync(request.TreasureItemId, cancellationToken);
        if (treasureItem == null)
        {
            throw new ArgumentException("Treasure item not found");
        }

        // Check if already collected
        if (treasureItem.IsCollected)
        {
            throw new InvalidOperationException("This item has already been collected");
        }

        // Prevent users from collecting their own items
        if (treasureItem.PlacedByUserId == request.UserId)
        {
            throw new InvalidOperationException("You cannot collect your own treasure items");
        }

        // Validate proximity
        var distance = _locationService.CalculateDistance(
            request.UserLatitude,
            request.UserLongitude,
            treasureItem.Latitude,
            treasureItem.Longitude);

        if (distance > treasureItem.DiscoveryRadiusMeters)
        {
            throw new InvalidOperationException($"You are too far away. Distance: {distance:F2}m, Required: {treasureItem.DiscoveryRadiusMeters}m");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        // Update treasure item
        treasureItem.IsCollected = true;
        treasureItem.CollectedByUserId = request.UserId;
        treasureItem.CollectedAt = DateTime.UtcNow;
        await _treasureRepository.UpdateAsync(treasureItem, cancellationToken);

        // Add to inventory
        var inventoryItem = new UserInventory
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TreasureItemId = treasureItem.Id,
            CollectedAt = treasureItem.CollectedAt.Value,
            PointsEarned = treasureItem.PointValue
        };
        await _inventoryRepository.AddAsync(inventoryItem, cancellationToken);

        // Update user points
        user.TotalPoints += treasureItem.PointValue;
        user.LastActiveAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserInventoryDto
        {
            Id = inventoryItem.Id,
            UserId = inventoryItem.UserId,
            TreasureItemId = inventoryItem.TreasureItemId,
            TreasureItemName = treasureItem.Name,
            CollectedAt = inventoryItem.CollectedAt,
            PointsEarned = inventoryItem.PointsEarned
        };
    }
}
