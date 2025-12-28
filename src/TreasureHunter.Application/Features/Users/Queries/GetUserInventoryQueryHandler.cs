using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TreasureHunter.Application.DTOs;
using TreasureHunter.Domain.Entities;
using TreasureHunter.Domain.Interfaces;

namespace TreasureHunter.Application.Features.Users.Queries;

public class GetUserInventoryQueryHandler : IRequestHandler<GetUserInventoryQuery, IEnumerable<UserInventoryDto>>
{
    private readonly IRepository<UserInventory> _inventoryRepository;

    public GetUserInventoryQueryHandler(IRepository<UserInventory> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IEnumerable<UserInventoryDto>> Handle(GetUserInventoryQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.FindAsync(i => i.UserId == request.UserId, cancellationToken);

        return inventory.Select(item => new UserInventoryDto
        {
            Id = item.Id,
            UserId = item.UserId,
            TreasureItemId = item.TreasureItemId,
            CollectedAt = item.CollectedAt,
            PointsEarned = item.PointsEarned
        }).OrderByDescending(i => i.CollectedAt).ToList();
    }
}
