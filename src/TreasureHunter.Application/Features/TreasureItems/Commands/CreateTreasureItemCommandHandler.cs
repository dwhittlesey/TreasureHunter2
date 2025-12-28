using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Domain.Constants;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.Application.Features.TreasureItems.Commands;

public class CreateTreasureItemCommandHandler : IRequestHandler<CreateTreasureItemCommand, TreasureItemDto>
{
    private readonly IRepository<TreasureItem> _treasureRepository;
    private readonly IRepository<ItemType> _itemTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTreasureItemCommandHandler(
        IRepository<TreasureItem> treasureRepository,
        IRepository<ItemType> itemTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _treasureRepository = treasureRepository;
        _itemTypeRepository = itemTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TreasureItemDto> Handle(CreateTreasureItemCommand request, CancellationToken cancellationToken)
    {
        // Validate item type exists
        var itemType = await _itemTypeRepository.GetByIdAsync(request.ItemTypeId, cancellationToken);
        if (itemType == null)
        {
            throw new ArgumentException("Invalid item type ID");
        }

        // Validate discovery radius
        if (request.DiscoveryRadiusMeters < GameConstants.MinDiscoveryRadiusMeters ||
            request.DiscoveryRadiusMeters > GameConstants.MaxDiscoveryRadiusMeters)
        {
            throw new ArgumentException($"Discovery radius must be between {GameConstants.MinDiscoveryRadiusMeters} and {GameConstants.MaxDiscoveryRadiusMeters} meters");
        }

        var treasureItem = new TreasureItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ItemTypeId = request.ItemTypeId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Altitude = request.Altitude,
            DiscoveryRadiusMeters = request.DiscoveryRadiusMeters,
            PointValue = itemType.BasePointValue,
            PlacedByUserId = request.PlacedByUserId,
            PlacedAt = DateTime.UtcNow,
            ModelUrl = itemType.DefaultModelUrl,
            IconUrl = itemType.DefaultIconUrl
        };

        await _treasureRepository.AddAsync(treasureItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TreasureItemDto
        {
            Id = treasureItem.Id,
            Name = treasureItem.Name,
            Description = treasureItem.Description,
            ItemTypeId = treasureItem.ItemTypeId,
            ItemTypeName = itemType.Name,
            Latitude = treasureItem.Latitude,
            Longitude = treasureItem.Longitude,
            Altitude = treasureItem.Altitude,
            DiscoveryRadiusMeters = treasureItem.DiscoveryRadiusMeters,
            PointValue = treasureItem.PointValue,
            IsCollected = treasureItem.IsCollected,
            PlacedByUserId = treasureItem.PlacedByUserId,
            PlacedAt = treasureItem.PlacedAt,
            ModelUrl = treasureItem.ModelUrl,
            IconUrl = treasureItem.IconUrl
        };
    }
}
