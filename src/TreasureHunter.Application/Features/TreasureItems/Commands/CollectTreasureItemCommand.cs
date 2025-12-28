using System;
using MediatR;
using TreasureHunter.Application.DTOs;

namespace TreasureHunter.Application.Features.TreasureItems.Commands;

public class CollectTreasureItemCommand : IRequest<UserInventoryDto>
{
    public Guid TreasureItemId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public double UserLatitude { get; set; }
    public double UserLongitude { get; set; }
}
